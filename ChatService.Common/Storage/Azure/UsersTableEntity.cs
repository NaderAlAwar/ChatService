using System;
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

        public DateTime TicksToDateTime()
        {
            string temp;
            if (RowKey.Contains("ticks_"))
            {
                temp = RowKey.Replace("ticks_", "");
            }
            else
            {
                temp = dateTime.Replace("ticks_", "");
            }
            long ticks = Convert.ToInt64(temp);
            return new DateTime(ticks);
        }
    }
}