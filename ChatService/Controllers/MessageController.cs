using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Providers;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        private readonly IMessageStore messagesStore;
        private readonly ILogger<MessageController> logger;
        private readonly ITimeProvider timeProvider;

        public MessageController(IMessageStore messagesStore, ILogger<MessageController> logger, ITimeProvider timeProvider)
        {
            this.messagesStore = messagesStore;
            this.logger = logger;
            this.timeProvider = timeProvider;
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> ListMessages(string conversationId)
        {
            try
            {
                IEnumerable<Message> messages = await messagesStore.ListMessages(conversationId);
                IEnumerable<ListMessagesItemDto> dtos =
                    messages.Select(m => new ListMessagesItemDto(m.Text, m.SenderUsername, m.UtcTime));
                return Ok(new ListMessagesDto(dtos));
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e,
                    "Could not reach storage to list messages, conversationId {conversationId}", conversationId);
                return StatusCode(503, "Failed to reach storage");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e,
                    "Failed to list messages of conversation {conversationId}", conversationId);
                return StatusCode(500, "Failed to retrieve messages");
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> PostMessage(string id, [FromBody] SendMessageDto messageDto)
        {
            var message = new Message(messageDto.Text, messageDto.SenderUsername, timeProvider.GetCurrentTimeUtc());
            try
            {
                await messagesStore.AddMessage(id, message);

                logger.LogInformation(Events.ConversationMessageAdded,
                    "Message has been added to conversation {conversationId}, sender: {senderUsername}", id, messageDto.SenderUsername);
                return Ok(message);
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e,
                    "Could not reach storage to add message, conversationId {conversationId}", id);
                return StatusCode(503, "Failed to reach storage");
            }
            catch (DuplicateMessageException)
            {
                logger.LogInformation(Events.MessageAlreadyExists,
                    $"The message for conversation {id} and timestamp {message.UtcTime} cannot be created because it already exists");
                return StatusCode(409, "Message already exists");
            }
            catch (ArgumentException) {
                return StatusCode(400, "Null conversationId and/or incomplete request body");
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e,
                    "Failed to add message to conversation, conversationId: {conversationId}", id);
                return StatusCode(500, "Failed to add message");
            }
        }
    }
}