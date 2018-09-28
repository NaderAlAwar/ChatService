using System.Collections.Generic;

namespace ChatService.DataContracts
{
    public class ListConversationsDto
    {
        public ListConversationsDto(List<ListConversationsItemDto> conversations)
        {
            Conversations = conversations;
        }

        public List<ListConversationsItemDto> Conversations { get; }
    }
}
