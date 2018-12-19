using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Logging;
using ChatService.Notifications;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationsStore conversationsStore;
        private readonly ILogger<ConversationService> logger;
        private readonly INotificationService notificationService;

        public ConversationService(IConversationsStore conversationsStore, ILogger<ConversationService> logger, INotificationService notificationService)
        {
            this.conversationsStore = conversationsStore;
            this.logger = logger;
            this.notificationService = notificationService;
        }

        public async Task<Message> HandlePostMessageRequest(string conversationId, SendMessageDto messageDto)
        {
            var currentTime = DateTime.Now;
            string messageId = GenerateMessageId(messageDto, currentTime);
            var messageDtoV2 = new SendMessageDtoV2(messageDto.Text, messageDto.SenderUsername, messageId);

            return await HandlePostMessageRequest(conversationId, messageDtoV2);

        }

        public async Task<Message> HandlePostMessageRequest(string conversationId, SendMessageDtoV2 messageDto)
        {
            var matchingMessage = await conversationsStore.TryGetMessage(conversationId, messageDto.MessageId);

            if (matchingMessage.Item1 == true) // if the message was found in storage
            {
                return matchingMessage.Item2;
            }

            var currentTime = DateTime.Now;
            var message = new Message(messageDto.Text, messageDto.SenderUsername, currentTime);


            await conversationsStore.AddMessage(conversationId, messageDto.MessageId, message);

            logger.LogInformation(Events.ConversationMessageAdded,
                "Message {MessageId} has been added to conversation {conversationId}, sender: {senderUsername}", messageDto.MessageId ,conversationId, messageDto.SenderUsername);

            var conversation = await conversationsStore.GetConversation(messageDto.SenderUsername, conversationId);
            var usersToNotify = conversation.Participants;
            var newMessagePayload = new NotificationPayload(currentTime, "MessageAdded", conversationId, usersToNotify);

            await notificationService.SendNotificationAsync(newMessagePayload);

            return message;
        }

        public async Task<ListMessagesDto> HandleListMessagesRequest(string conversationId, [FromQuery] string startCt,
            [FromQuery] string endCt, [FromQuery] int limit)
        {
            var messagesWindow = await conversationsStore.ListMessages(conversationId, startCt, endCt, limit);
            List<ListMessagesItemDto> dtos =
                (messagesWindow.Messages.Select(m => new ListMessagesItemDto(m.Text, m.SenderUsername, m.UtcTime))).ToList();

            string newStartCt = messagesWindow.StartCt;
            string newEndCt = messagesWindow.EndCt;

            string nextUri = (string.IsNullOrEmpty(newStartCt)) ? "" : $"api/conversation/{conversationId}?startCt={newStartCt}&limit={limit}";
            string previousUri = (string.IsNullOrEmpty(newEndCt)) ? "" : $"api/conversation/{conversationId}?endCt={newEndCt}&limit={limit}";

            return new ListMessagesDto(dtos, nextUri, previousUri);
        }

        private string GenerateMessageId(SendMessageDto messageDto, DateTime currentTime)
        {
            return $"{messageDto.SenderUsername}_{currentTime.Ticks}";
        }
    }
}