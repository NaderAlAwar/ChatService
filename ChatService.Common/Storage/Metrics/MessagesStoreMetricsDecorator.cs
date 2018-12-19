using Microsoft.Extensions.Logging.Metrics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Storage.Metrics
{
    public class MessagesStoreMetricsDecorator : IMessagesStore
    {
        private readonly IMessagesStore store;
        private readonly AggregateMetric addMessageMetric;
        private readonly AggregateMetric getMessageMetric;
        private readonly AggregateMetric listMessagesMetric;

        public MessagesStoreMetricsDecorator(IMessagesStore store, IMetricsClient metricsClient)
        {
            this.store = store;

            addMessageMetric = metricsClient.CreateAggregateMetric("AddMessageToMessageStoreTime");
            getMessageMetric = metricsClient.CreateAggregateMetric("GetMessageFromMessageStoreTime");
            listMessagesMetric = metricsClient.CreateAggregateMetric("ListMessagesFromMessageStoreTime");
        }

        public Task AddMessage(string conversationId, string messageId, Message message)
        {
            return addMessageMetric.TrackTime(() => store.AddMessage(conversationId, messageId, message));
        }

        public Task<Message> GetMessage(string conversationId, string messageId)
        {
            return getMessageMetric.TrackTime(() => store.GetMessage(conversationId, messageId));
        }

        public Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            return listMessagesMetric.TrackTime(() => store.ListMessages(conversationId, startCt, endCt, limit));
        }
    }
}
