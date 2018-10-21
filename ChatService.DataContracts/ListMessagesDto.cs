using System.Collections.Generic;
using System.Linq;

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
