using ChatService.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IConversationsStore
    {
        /// <returns>The list of all conversations of a given user sorted by the last time the conversation was modified (recent conversations first)</returns>
        Task<IEnumerable<Conversation>> ListConversations(string username);

        Task AddConversation(Conversation conversation);

        Task<IEnumerable<Message>> ListMessages(string conversationId);

        Task AddMessage(string conversationId, Message message);
    }
}
