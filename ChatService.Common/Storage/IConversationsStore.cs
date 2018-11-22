using System.Collections.Generic;
using System.Threading.Tasks;
using ChatService.Storage.Azure;

namespace ChatService.Storage
{
    public interface IConversationsStore : IMessagesStore // checkout the Interface segregation principle
    {
        /// <returns>The list of all conversations of a given user sorted by the last time the conversation was modified (recent conversations first)</returns>
        Task<SortedConversationsWindow> ListConversations(string username, string startCt, string endCt, int limit);

        Task AddConversation(Conversation conversation);

        //Task<bool> TryDeleteConversation(string conversationId);
    }
}