using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IMessagesStore
    {
        Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit);

        Task AddMessage(string conversationId, string messageId, Message message);

        Task<Tuple<bool, Message>> TryGetMessage(string conversationId, string messageId);
    }
}