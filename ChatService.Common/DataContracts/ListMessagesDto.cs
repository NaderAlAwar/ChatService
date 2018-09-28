using System.Collections.Generic;
using System.Linq;
using ChatService.Storage;

namespace ChatService.DataContracts
{
    public class ListMessagesDto
    {
        public ListMessagesDto(IEnumerable<ListMessagesItemDto> messages)
        {
            Messages = messages.ToList();
        }

        public List<ListMessagesItemDto> Messages { get; }
    }
}
