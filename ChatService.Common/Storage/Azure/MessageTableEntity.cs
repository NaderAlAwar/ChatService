using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class MessageTableEntity : TableEntity
    {
        public MessageTableEntity()
        {
        }

        public string SenderUsername { get; set; }
        public string Text { get; set; }
    }
}