using System;
using ChatService.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IConversationsStore
    {
        Task<IEnumerable<Conversation>> ListConversations(string username);

        Task AddConversation(Conversation conversation);

        Task UpdateConversation(string conversationId, DateTime newTimeStamp);
    }
}
