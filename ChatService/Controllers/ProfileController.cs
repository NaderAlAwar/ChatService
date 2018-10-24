using System;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IProfileStore profileStore;
        private readonly ILogger<ProfileController> logger;
        private readonly IMetricsClient metricsClient;
        private readonly AggregateMetric getProfileControllerTimeMetric;
        private readonly AggregateMetric createProfileControllerTimeMetric;

        public ProfileController(IProfileStore profileStore, ILogger<ProfileController> logger, IMetricsClient metricsClient)
        {
            this.profileStore = profileStore;
            this.logger = logger;
            this.metricsClient = metricsClient;
            getProfileControllerTimeMetric = this.metricsClient.CreateAggregateMetric("GetProfileControllerTime");
            createProfileControllerTimeMetric = this.metricsClient.CreateAggregateMetric("CreateProfileControllerTime");
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto request)
        {
            var profile = new UserProfile(request.Username, request.FirstName, request.LastName);
            try
            {
                return await createProfileControllerTimeMetric.TrackTime(async () =>
                {
                    await profileStore.AddProfile(profile);
                    logger.LogInformation(Events.ProfileCreated, "A Profile has been added for user {username}",
                        request.Username);
                    return Created(request.Username, profile);
                });
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, "Failed to create a profile for user {username}", request.Username);

                return StatusCode(503, "Failed to reach storage");
            }
            catch (DuplicateProfileException)
            {
                logger.LogInformation(Events.ProfileAlreadyExists, 
                    "The profile for user {username} cannot be created because it already exists",
                    request.Username);
                return StatusCode(409, "Profile already exists");
            }
            catch (ArgumentException)
            {
                return StatusCode(400, "Invalid or incomplete Request Body");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, "Failed to create a profile for user {username}", request.Username);
                return StatusCode(500, "Failed to create profile");
            }
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            try
            {
                return await getProfileControllerTimeMetric.TrackTime(async () =>
                {
                    UserProfile profile = await profileStore.GetProfile(username);
                    return Ok(profile);
                });
            }
            catch (ProfileNotFoundException)
            {
                logger.LogInformation(Events.ProfileNotFound,
                    "A profile was request for user {username} but was not found", username);
                return NotFound($"Profile for user {username} was not found");
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, "Failed to retrieve profile of user {username}", username);
                return StatusCode(503, "Failed to reach storage");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occured while retrieving a user profile");
                return StatusCode(500, "Failed to retrieve profile of user {username}");
            }
        }
    }
}
