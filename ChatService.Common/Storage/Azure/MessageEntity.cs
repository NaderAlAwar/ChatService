using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class MessageEntity : TableEntity
    {
        public string Text { get; set; }
        public string SenderUsername { get; set; }
        public string MessageId { get; set; }
    }
}