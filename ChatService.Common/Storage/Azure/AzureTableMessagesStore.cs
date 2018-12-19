using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureTableMessagesStore : IMessagesStore
    {
        private readonly ICloudTable table;

        public AzureTableMessagesStore(ICloudTable cloudTable)
        {
            this.table = cloudTable;
        }

        public async Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            string high = string.IsNullOrEmpty(startCt) ? DateTime.MaxValue.Ticks.ToString() : startCt;
            string low = string.IsNullOrEmpty(endCt) ? DateTime.MinValue.Ticks.ToString() : endCt;

            TableQuery<MessageEntity> query = new TableQuery<MessageEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversationId),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan,
                            low),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan,
                            high)
                    )
                ));

            query.TakeCount = limit;

            try
            {
                var results = await table.ExecuteQuery(query);
                List<MessageEntity> entities = results.Results;

                string newStartCt = "", newEndCt = "";
                if (entities.Count > 0)
                {
                    newStartCt = entities.First().RowKey;
                    newEndCt = entities.Last().RowKey;
                }

                IEnumerable<Message> messageList = entities.Select(entity =>
                {
                    long ticks = long.Parse(entity.RowKey);
                    ticks = DateTimeUtils.InvertTicks(ticks);
                    var utcTime = new DateTime(ticks);
                    return new Message(entity.Text, entity.SenderUsername, utcTime);
                });

                return new SortedMessagesWindow(messageList, newStartCt, newEndCt);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Failed to list messages", e);
            }
        }

        public async Task AddMessage(string conversationId, string messageId, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.SenderUsername))
            {
                throw new ArgumentNullException(nameof(message.SenderUsername));
            }

            MessageEntity entity = new MessageEntity
            {
                PartitionKey = conversationId,
                RowKey = DateTimeUtils.FromDateTimeToInvertedString(message.UtcTime),
                SenderUsername = message.SenderUsername,
                Text = message.Text,
                MessageId = messageId
            };

            var insertOperation = TableOperation.Insert(entity);
            try
            {
                await table.ExecuteAsync(insertOperation);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // conflict
                {
                    // TODO: the caller should retry with a different timestamp
                    // The exception should be more explicit to allow the caller to distinguish between
                    // storage is down and a conflict
                    throw new StorageErrorException("Race conditions between messages", e);
                }
                throw new StorageErrorException("Could not write to Azure Table", e);
            }
        }

        public async Task<Message> GetMessage(string conversationId, string messageId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            TableQuery<MessageEntity> query = new TableQuery<MessageEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversationId));

            try
            {
                var results = await table.ExecuteQuery(query);
                List<MessageEntity> entities = results.Results;

                foreach (var messageEntity in entities)
                {
                    if (messageEntity.MessageId == messageId)
                    {
                        long ticks = long.Parse(messageEntity.RowKey);
                        ticks = DateTimeUtils.InvertTicks(ticks);
                        var utcTime = new DateTime(ticks);
                        return new Message(messageEntity.Text, messageEntity.SenderUsername, utcTime);
                    }
                }

                throw new MessageNotFoundException($"Could not find a message with id {messageId}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}