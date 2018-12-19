using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Storage;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Services
{
    public class ConversationServiceMetricsDecorator : IConversationService
    {
        private readonly IConversationService service;
        private readonly AggregateMetric listMessagesControllerTimeMetric;
        private readonly AggregateMetric postMessageControllerTimeMetric;

        public ConversationServiceMetricsDecorator(IConversationService service, IMetricsClient metricsClient)
        {
            this.service = service;

            listMessagesControllerTimeMetric = metricsClient.CreateAggregateMetric("ListMessagesControllerTime");
            postMessageControllerTimeMetric = metricsClient.CreateAggregateMetric("PostMessageControllerTime");
        }

        public Task<Message> PostMessage(string conversationId, SendMessageDto messageDto)
        {
            return postMessageControllerTimeMetric.TrackTime(() => service.PostMessage(conversationId, messageDto));
        }

        public Task<Message> PostMessage(string conversationId, SendMessageDtoV2 messageDto)
        {
            return postMessageControllerTimeMetric.TrackTime(() => service.PostMessage(conversationId, messageDto));
        }

        public Task<ListMessagesDto> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            return listMessagesControllerTimeMetric.TrackTime(() => service.ListMessages(conversationId, startCt, endCt, limit));
        }
    }
}
