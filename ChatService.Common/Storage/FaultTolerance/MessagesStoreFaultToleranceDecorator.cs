﻿using System.Threading.Tasks;
using Polly;

namespace ChatService.Storage.FaultTolerance
{
    public class MessagesStoreFaultToleranceDecorator : IMessagesStore
    {
        private readonly IMessagesStore store;
        private readonly ISyncPolicy faultTolerancePolicy;

        public MessagesStoreFaultToleranceDecorator(IMessagesStore store, ISyncPolicy faultTolerancePolicy)
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

        public Task AddMessage(string conversationId, Message message)
        {
            return faultTolerancePolicy.Execute(
                async () => await store.AddMessage(conversationId, message)
            );
        }
    }
}