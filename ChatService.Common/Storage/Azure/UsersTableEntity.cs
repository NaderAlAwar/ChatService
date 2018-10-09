using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class UsersTableEntity : TableEntity
    {
        public UsersTableEntity()
        {

        }

        public string dateTime { get; set; }
        public string recipient { get; set; }
        public string conversationId { get; set; }
    }
}