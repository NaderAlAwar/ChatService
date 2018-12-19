using System;
using System.Threading.Tasks;
using Polly;

namespace ChatService.Storage.FaultTolerance
{
    public class ConversationsStoreFaultToleranceDecorator : IConversationsStore
    {
        private readonly IConversationsStore store;
        private readonly ISyncPolicy faultTolerancePolicy;

        public ConversationsStoreFaultToleranceDecorator(IConversationsStore store, ISyncPolicy faultTolerancePolicy)
        {
            this.store = store;
            this.faultTolerancePolicy = faultTolerancePolicy;
        }

        public Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.ListMessages(conversationId, startCt, endCt, limit)
            );
        }

        public Task AddMessage(string conversationId, string messageId, Message message)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.AddMessage(conversationId, messageId, message)
            );
        }

        public Task<Tuple<bool,Message>> TryGetMessage(string conversationId, string messageId)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.TryGetMessage(conversationId, messageId)
            );
        }

        public Task<SortedConversationsWindow> ListConversations(string username, string startCt, string endCt, int limit)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.ListConversations(username, startCt, endCt, limit)
            );
        }

        public Task AddConversation(Conversation conversation)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.AddConversation(conversation)
            );
        }

        public Task<Conversation> GetConversation(string username, string conversationId)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.GetConversation(username, conversationId)
            );
        }
    }
}