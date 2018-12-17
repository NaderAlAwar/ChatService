using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Notifications;
using ChatService.Storage;
using ChatService.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;
using Polly;
using Polly.Wrap;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class ConversationController : Controller
    {
        private readonly IConversationsStore conversationsStore;
        private readonly ILogger<ConversationController> logger;
        private readonly IMetricsClient metricsClient;
        private readonly INotificationService notificationService;
        private readonly AggregateMetric postMessageControllerTimeMetric;
        private readonly AggregateMetric listMessagesControllerTimeMetric;

        public ConversationController(IConversationsStore conversationsStore, ILogger<ConversationController> logger, IMetricsClient metricsClient,
            INotificationService notificationService)
        {
            this.conversationsStore = conversationsStore;
            this.logger = logger;
            this.metricsClient = metricsClient;
            this.notificationService = notificationService;
            listMessagesControllerTimeMetric = this.metricsClient.CreateAggregateMetric("ListMessagesControllerTime");
            postMessageControllerTimeMetric = this.metricsClient.CreateAggregateMetric("PostMessageControllerTime");
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> ListMessages(string conversationId, [FromQuery] string startCt, [FromQuery] string endCt, [FromQuery] int limit)
        {
            try
            {
                return await listMessagesControllerTimeMetric.TrackTime(async () =>
                {
                    var messagesWindow = await conversationsStore.ListMessages(conversationId, startCt, endCt, limit);
                    List<ListMessagesItemDto> dtos =
                        (messagesWindow.Messages.Select(m => new ListMessagesItemDto(m.Text, m.SenderUsername, m.UtcTime))).ToList();
                        
                    string newStartCt = messagesWindow.StartCt;
                    string newEndCt = messagesWindow.EndCt;

                    string nextUri = (string.IsNullOrEmpty(newStartCt)) ? "" : $"api/conversation/{conversationId}?startCt={newStartCt}&limit={limit}";
                    string previousUri = (string.IsNullOrEmpty(newEndCt)) ? "" : $"api/conversation/{conversationId}?endCt={newEndCt}&limit={limit}";

                    return Ok(new ListMessagesDto(dtos, nextUri, previousUri));
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
                    var currentTime = DateTime.Now;
                    var message = new Message(messageDto.Text, messageDto.SenderUsername, currentTime);
                    await conversationsStore.AddMessage(id, message);

                    logger.LogInformation(Events.ConversationMessageAdded,
                        "Message has been added to conversation {conversationId}, sender: {senderUsername}", id, messageDto.SenderUsername);

                    var conversation = await conversationsStore.GetConversation(messageDto.SenderUsername, id);
                    var usersToNotify = conversation.Participants;
                    var newMessagePayload = new NotificationPayload(currentTime, "MessageAdded", id, usersToNotify);

                    await notificationService.SendNotificationAsync(newMessagePayload);
                    
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