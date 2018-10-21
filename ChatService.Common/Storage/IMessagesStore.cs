using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IMessagesStore
    {
        Task<IEnumerable<Message>> ListMessages(string conversationId);

        Task AddMessage(string conversationId, Message message);
    }
}