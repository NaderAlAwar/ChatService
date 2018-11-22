using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatService.Storage
{
    public interface IMessagesStore
    {
        Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit);

        Task AddMessage(string conversationId, Message message);
    }
}