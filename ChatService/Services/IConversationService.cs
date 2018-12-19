using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Services
{
    public interface IConversationService
    {
        Task<Message> HandlePostMessageRequest(string conversationId, SendMessageDto messageDto);
        Task<Message> HandlePostMessageRequest(string conversationId, SendMessageDtoV2 messageDto);
        Task<ListMessagesDto> HandleListMessagesRequest(string conversationId, [FromQuery] string startCt,
            [FromQuery] string endCt, [FromQuery] int limit);
    }
}