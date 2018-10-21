using System;
using ChatService.Utils;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class OrderedConversationEntity : TableEntity
    {
        public string ConversationId { get; set; }
        public string Participants { get; set; }

        private const string TicksPrefix = "ticks__";

        public static readonly string MinRowKey = TicksToRowKey(DateTime.MinValue.Ticks);
        public static readonly string MaxRowKey = TicksToRowKey(DateTime.MaxValue.Ticks);

        public OrderedConversationEntity()
        {
        }

        public OrderedConversationEntity(string username, string conversationId, string[] participants, DateTime lastModifiedDateUtc)
        {
            PartitionKey = username;
            RowKey = ToRowKey(lastModifiedDateUtc);
            ConversationId = conversationId;
            Participants = string.Join(",", participants);
        }

        public static string ToRowKey(DateTime dateTime)
        {
            long ticks = DateTimeUtils.InvertTicks(dateTime.Ticks);
            return TicksToRowKey(ticks);
        }

        private static string TicksToRowKey(long ticks)
        {
            return TicksPrefix + ticks.ToString("d19");
        }

        public static DateTime ToDateTime(string rowKey)
        {
            if (rowKey.Length < TicksPrefix.Length)
            {
                throw new ArgumentException($"Invalid row key {rowKey}");
            }
            string ticksStr = rowKey.Substring(TicksPrefix.Length);
            if (long.TryParse(ticksStr, out long ticks))
            {
                ticks = DateTimeUtils.InvertTicks(ticks);
                return new DateTime(ticks);
            }
            throw new InvalidOperationException($"Invalid ticks row key {rowKey}");
        }

        public DateTime GetLastModifiedDateTimeUtc()
        {
            return ToDateTime(RowKey);
        }

        public string[] GetParticipants()
        {
            return Participants.Split(',');
        }
    }
}