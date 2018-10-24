using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ConversationController : Controller
    {
        private readonly IConversationsStore conversationsStore;
        private readonly ILogger<ConversationController> logger;
        private readonly IMetricsClient metricsClient;
        private readonly AggregateMetric postMessageControllerTimeMetric;
        private readonly AggregateMetric listMessagesControllerTimeMetric;

        public ConversationController(IConversationsStore conversationsStore, ILogger<ConversationController> logger, IMetricsClient metricsClient)
        {
            this.conversationsStore = conversationsStore;
            this.logger = logger;
            this.metricsClient = metricsClient;
            listMessagesControllerTimeMetric = this.metricsClient.CreateAggregateMetric("ListMessagesControllerTime");
            postMessageControllerTimeMetric = this.metricsClient.CreateAggregateMetric("PostMessageControllerTime");
        }

        //TODO: add paging
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> ListMessages(string conversationId)
        {
            try
            {
                return await listMessagesControllerTimeMetric.TrackTime(async () =>
                {
                    IEnumerable<Message> messages = await conversationsStore.ListMessages(conversationId);
                    IEnumerable<ListMessagesItemDto> dtos =
                        messages.Select(m => new ListMessagesItemDto(m.Text, m.SenderUsername, m.UtcTime));

                    return Ok(new ListMessagesDto(dtos));
                });
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, 
                    "Could not reach storage to list messages, conversationId {conversationId}", conversationId);
                return StatusCode(503);
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, 
                    "Failed to list messages of conversation {conversationId}", conversationId);
                return StatusCode(500);
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> PostMessage(string id, [FromBody] SendMessageDto messageDto)
        {
            try
            {
                return await postMessageControllerTimeMetric.TrackTime(async () =>
                {
                    var message = new Message(messageDto.Text, messageDto.SenderUsername, DateTime.UtcNow);
                    await conversationsStore.AddMessage(id, message);

                    logger.LogInformation(Events.ConversationMessageAdded,
                        "Message has been added to conversation {conversationId}, sender: {senderUsername}", id, messageDto.SenderUsername);
                    return Ok(message);
                });
            }
            catch (StorageErrorException e)
            {
                logger.LogError(Events.StorageError, e, 
                    "Could not reach storage to add message, conversationId {conversationId}", id);
                return StatusCode(503);
            }
            catch (Exception e)
            {
                logger.LogError(Events.InternalError, e, 
                    "Failed to add message to conversation, conversationId: {conversationId}", id);
                return StatusCode(500);
            }
        }
    }
}