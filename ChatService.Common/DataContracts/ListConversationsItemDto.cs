using System;

namespace ChatService.DataContracts
{
    public class ListConversationsItemDto
    {
        public ListConversationsItemDto(string id, UserInfoDto recipient, DateTime lastModifiedDateUtc)
        {
            Id = id;
            Recipient = recipient;
            LastModifiedDateUtc = lastModifiedDateUtc;
        }

        public string Id { get; }
        public UserInfoDto Recipient { get; }
        public DateTime LastModifiedDateUtc { get; }
    }
}
