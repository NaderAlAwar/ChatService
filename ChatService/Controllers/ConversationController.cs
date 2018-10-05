using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class ConversationController : Controller
    {
        private readonly IConversationsStore conversationsStore;
        private readonly IProfileStore profileStore;
        private readonly ILogger<ConversationController> logger;

        public ConversationController(IConversationsStore conversationsStore, IProfileStore profileStore,
            ILogger<ConversationController> logger)
        {
            this.conversationsStore = conversationsStore;
            this.profileStore = profileStore;
            this.logger = logger;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> ListConversations(string username)
        {
            try
            {
                var conversations = await conversationsStore.ListConversations(username);

                var conversationList = new List<ListConversationsItemDto>();
                foreach (var conversation in conversations)
                {
                    string recipientUserName = conversation.Participants.Except(new[] { username }).First();
                    UserProfile profile = await profileStore.GetProfile(recipientUserName);
                    var recipientInfo = new UserInfoDto(profile.Username, profile.FirstName, profile.LastName);
                    conversationList.Add(new ListConversationsItemDto(conversation.Id, recipientInfo, conversation.LastModifiedDateUtc));
                }
                return Ok(new ListConversationsDto(conversationList));
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
                string id = GenerateConversationId(conversationDto);
                Conversation conversation = new Conversation(id, conversationDto.Participants, DateTime.UtcNow);
                await conversationsStore.AddConversation(conversation);

                logger.LogInformation(Events.ConversationCreated, "Conversation with id {conversationId} was created");
                return Ok(conversation);
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