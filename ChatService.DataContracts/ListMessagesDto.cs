using System.Collections.Generic;
using System.Linq;

namespace ChatService.DataContracts
{
    public class ListMessagesDto
    {
        public ListMessagesDto(IEnumerable<ListMessagesItemDto> messages, string nextUri, string previousUri)
        {
            Messages = messages.ToList();
        }

        public List<ListMessagesItemDto> Messages { get; }
        public string NextUri { get; }
        public string PreviousUri { get; }
    }
}
