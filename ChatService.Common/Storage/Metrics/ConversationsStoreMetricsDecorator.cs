using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatService.Storage.Azure;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Storage.Metrics
{
    public class ConversationStoreMetricsDecorator : IConversationsStore
    {
        private readonly IConversationsStore store;
        private readonly AggregateMetric listMessagesMetric;
        private readonly AggregateMetric addMessageMetric;
        private readonly AggregateMetric getMessageMetric;
        private readonly AggregateMetric addConversationMetric;
        private readonly AggregateMetric listConversationsMetric;
        private readonly AggregateMetric getConversationMetric;

        public ConversationStoreMetricsDecorator(IConversationsStore store, IMetricsClient metricsClient)
        {
            this.store = store;

            listMessagesMetric = metricsClient.CreateAggregateMetric("ListMessagesfromConversationsStoreTime");
            addMessageMetric = metricsClient.CreateAggregateMetric("AddMessageToConversationsStoreTime");
            getMessageMetric = metricsClient.CreateAggregateMetric("GetMessageFromConversationsStoreTime");
            listConversationsMetric = metricsClient.CreateAggregateMetric("ListConversationsTime");
            addConversationMetric = metricsClient.CreateAggregateMetric("AddConversationTime");
            getConversationMetric = metricsClient.CreateAggregateMetric("GetConversationsTime");
        }

        public Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            return listMessagesMetric.TrackTime(() => store.ListMessages(conversationId, startCt, endCt, limit));
        }

        public Task AddMessage(string conversationId, string messageId, Message message)
        {
            return addMessageMetric.TrackTime(() => store.AddMessage(conversationId, messageId, message));
        }

        public Task<Tuple<bool,Message>> TryGetMessage(string conversationId, string messageId)
        {
            return getMessageMetric.TrackTime(() => store.TryGetMessage(conversationId, messageId));
        }

        public Task<SortedConversationsWindow> ListConversations(string username, string startCt, string endCt, int limit)
        {
            return listConversationsMetric.TrackTime(() => store.ListConversations(username, startCt, endCt, limit));
        }

        public Task AddConversation(Conversation conversation)
        {
            return addConversationMetric.TrackTime(() => store.AddConversation(conversation));
        }

        public Task<Conversation> GetConversation(string username, string conversationId)
        {
            return getConversationMetric.TrackTime(() => store.GetConversation(username, conversationId));
        }
    }
}
