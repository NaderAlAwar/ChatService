using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IConversationsStore : IMessagesStore // checkout the Interface segregation principle
    {
        /// <returns>The list of all conversations of a given user sorted by the last time the conversation was modified (recent conversations first)</returns>
        Task<IEnumerable<Conversation>> ListConversations(string username);

        Task AddConversation(Conversation conversation);

        Task<Conversation> GetConversation(string username, string conversationId);

        //Task<bool> TryDeleteConversation(string conversationId);
    }
}