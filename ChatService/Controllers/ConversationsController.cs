using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Notifications;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class ConversationsController : Controller
    {
        private readonly IConversationsStore conversationsStore;
        private readonly IProfileStore profileStore;
        private readonly ILogger<ConversationsController> logger;
        private readonly IMetricsClient metricsClient;
        private readonly INotificationsService notificationsService;
        private readonly AggregateMetric listConversationsControllerTimeMetric;
        private readonly AggregateMetric createConversationControllerTimeMetric;

        public ConversationsController(IConversationsStore conversationsStore, IProfileStore profileStore,
            ILogger<ConversationsController> logger, IMetricsClient metricsClient, INotificationsService notificationsService)
        {
            this.conversationsStore = conversationsStore;
            this.profileStore = profileStore;
            this.logger = logger;
            this.metricsClient = metricsClient;
            this.notificationsService = notificationsService;
            listConversationsControllerTimeMetric = this.metricsClient.CreateAggregateMetric("ListConversationsControllerTime");
            createConversationControllerTimeMetric = this.metricsClient.CreateAggregateMetric("CreateConversationControllerTime");
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> ListConversations(string username, [FromQuery] string startCt, [FromQuery] string endCt, [FromQuery] int limit)
        {
            try
            {
                return await listConversationsControllerTimeMetric.TrackTime(async () =>
                {
                    var conversationsWindow = await conversationsStore.ListConversations(username, startCt, endCt, limit);

                    var conversationList = new List<ListConversationsItemDto>();
                    foreach (var conversation in conversationsWindow.Conversations)
                    {
                        string recipientUserName = conversation.Participants.Except(new[] { username }).First();
                        UserProfile profile = await profileStore.GetProfile(recipientUserName);
                        var recipientInfo = new UserInfoDto(profile.Username, profile.FirstName, profile.LastName);
                        conversationList.Add(new ListConversationsItemDto(conversation.Id, recipientInfo,
                            conversation.LastModifiedDateUtc));
                    }

                    string newStartCt = conversationsWindow.StartCt;
                    string newEndCt = conversationsWindow.EndCt;

                    string nextUri = (string.IsNullOrEmpty(newStartCt)) ? "" : $"api/conversations/{username}?startCt={newStartCt}&limit={limit}";
                    string previousUri = (string.IsNullOrEmpty(newEndCt)) ? "" : $"api/conversations/{username}?endCt={newEndCt}&limit={limit}";

                    return Ok(new ListConversationsDto(conversationList, nextUri, previousUri));
                });
            }
            catch (ProfileNotFoundException)
            {
                return NotFound($"Profile for user {username} was not found");
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, "Could not reach storage to list user conversations, username {username}", username);
                return StatusCode(503);
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, "Failed to retrieve conversations for user {username}", username);
                return StatusCode(500);
            }
        }

        [HttpPost()]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationDto conversationDto)
        {
            try
            {
                return await createConversationControllerTimeMetric.TrackTime(async () =>
                {
                    string id = GenerateConversationId(conversationDto);
                    var currentTime = DateTime.UtcNow;
                    Conversation conversation = new Conversation(id, conversationDto.Participants, currentTime);
                    await conversationsStore.AddConversation(conversation);

                    logger.LogInformation(Events.ConversationCreated, "Conversation with id {conversationId} was created");

                    var newConversationPayload = new NotificationPayload(currentTime, "ConversationAdded", id);
                    foreach (var user in conversationDto.Participants)
                    {
                        await notificationsService.SendNotificationAsync(user, newConversationPayload);
                    }

                    return Ok(conversation);
                });
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, "Could not reach storage to add user conversation");
                return StatusCode(503);
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, "Failed to add conversation");
                return StatusCode(500);
            }
        }

        private static string GenerateConversationId(CreateConversationDto conversationDto)
        {
            return string.Join("_", conversationDto.Participants.OrderBy(key => key));
        }
    }
}