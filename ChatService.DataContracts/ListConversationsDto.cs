using System.Collections.Generic;

namespace ChatService.DataContracts
{
    public class ListConversationsDto
    {
        public ListConversationsDto(List<ListConversationsItemDto> conversations, string nextUri, string previousUri)
        {
            Conversations = conversations;
            NextUri = nextUri;
            PreviousUri = previousUri;
        }

        public List<ListConversationsItemDto> Conversations { get; }
        public string NextUri { get; }
        public string PreviousUri { get; }
    }
}
