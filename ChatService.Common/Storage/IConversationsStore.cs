using ChatService.DataContracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IConversationsStore
    {
        Task<IEnumerable<Conversation>> ListConversations(string username);

        Task AddConversation(Conversation conversation);
<<<<<<< 32085f2f7d2ebf48b3f2b09b4aea4cba5e185473

        Task UpdateConversation(Conversation conversation);
=======
>>>>>>> Add the needed functionality to allow the saving of messages in Azure
    }
}
