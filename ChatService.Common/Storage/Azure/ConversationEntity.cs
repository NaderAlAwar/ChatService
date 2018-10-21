using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class ConversationEntity : TableEntity
    {
        public ConversationEntity()
        {
        }

        public ConversationEntity(string username, string conversationId, string[] participants, string ticksRowkey)
        {
            PartitionKey = username;
            RowKey = ToRowKey(conversationId);
            this.Participants = string.Join(",", participants);
            TicksRowKey = ticksRowkey;
        }

        public string Participants { get; set; }
        public string TicksRowKey { get; set; }

        private const string ConversationIdPrefix = "conversationId__";

        public static string ToRowKey(string conversationId)
        {
            return ConversationIdPrefix + conversationId;
        }

        public string GetConversationId()
        {
            if (RowKey.Length < ConversationIdPrefix.Length)
            {
                throw new ArgumentException($"Invalid row key for conversation Id {RowKey}");
            }
            return RowKey.Substring(ConversationIdPrefix.Length);
        }

        public string[] GetParticipants()
        {
            return Participants.Split(',');
        }

        public DateTime GetLastModifiedDateTimeUtc()
        {
            return OrderedConversationEntity.ToDateTime(TicksRowKey);
        }
    }
}