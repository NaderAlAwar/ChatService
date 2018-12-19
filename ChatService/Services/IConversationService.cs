using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services
{
    public interface IConversationService
    {
        Task<Message> PostMessage(string conversationId, SendMessageDto messageDto);
        Task<Message> PostMessage(string conversationId, SendMessageDtoV2 messageDto);
        Task<ListMessagesDto> ListMessages(string conversationId, [FromQuery] string startCt,
            [FromQuery] string endCt, [FromQuery] int limit);
    }
}