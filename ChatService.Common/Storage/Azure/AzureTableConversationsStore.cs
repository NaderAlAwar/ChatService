using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureTableConversationsStore : IConversationsStore
    {
        private readonly ICloudTable table;
        private const string rowkeyTsPrefix = "ticks_";

        public AzureTableConversationsStore(ICloudTable cloudTable)
        {
            table = cloudTable;
        }

        public async Task<IEnumerable<Conversation>> ListConversations(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            var result = new List<Conversation>();
            var query = new TableQuery<UsersTableEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, username),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, "ticks_0"),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, "ticks_99999999999999999999999"))));
            TableQuerySegment<UsersTableEntity> queryResult;
            try
            {
                queryResult = await table.ExecuteQuery(query);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not access Azure table");
            }
            foreach (var conversation in queryResult.Results)
            {
                var newConversation = new Conversation(conversation.conversationId, new[] { username, conversation.recipient }, ticksToDateTime(conversation.RowKey));
                result.Add(newConversation);
            }
//            result.Reverse();
            return result;
        }

        public async Task AddConversation(Conversation conversation)
        {
            validateConversation(conversation);

            var firstTableBatchOperation = await AddEntitiesForUser(conversation, conversation.Participants[0], conversation.Participants[1]);
            var secondTableBatchOperation = await AddEntitiesForUser(conversation, conversation.Participants[1], conversation.Participants[0]);

            try
            {
                await Task.WhenAll(table.ExecuteBatchAsync(firstTableBatchOperation),
                    table.ExecuteBatchAsync(secondTableBatchOperation));
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // the conversation already exists
                {
                    return;
                }
                throw new StorageErrorException("Could not write to azure table", e);
            }
        }

        private async Task<TableBatchOperation> AddEntitiesForUser(Conversation conversation, string userFrom, string userTo)
        {
            string timeInTicks = datetimeToTicks(conversation.LastModifiedDateUtc);
            var idEntity = new UsersTableEntity
            {
                PartitionKey = userFrom,
                RowKey = conversation.Id,
                dateTime = timeInTicks,
                recipient = userTo
            };

            var tsEntity = new UsersTableEntity()
            {
                PartitionKey = userFrom,
                RowKey = timeInTicks,
                conversationId = conversation.Id,
                recipient = userTo
            };

            var tableBatchOperation = new TableBatchOperation();
            tableBatchOperation.Insert(idEntity);
            tableBatchOperation.Insert(tsEntity);

            return tableBatchOperation;

        }

        private async Task<TableBatchOperation> UpdateEntitiesForUser(UsersTableEntity entityToUpdate, DateTime newTimeStamp)
        {
            var tableBatchOperation = new TableBatchOperation();

            string oldTimeStamp = entityToUpdate.dateTime;
            entityToUpdate.dateTime = datetimeToTicks(newTimeStamp);
            tableBatchOperation.Replace(entityToUpdate);

            var oldEntity = await retrieveEntity(entityToUpdate.PartitionKey, oldTimeStamp);
            tableBatchOperation.Delete(oldEntity);

            var newEntity = new UsersTableEntity
            {
                PartitionKey = entityToUpdate.PartitionKey,
                RowKey = datetimeToTicks(newTimeStamp),
                conversationId = entityToUpdate.RowKey,
                recipient = entityToUpdate.recipient
            };
            tableBatchOperation.Insert(newEntity);

            return tableBatchOperation;
        }

        public async Task UpdateConversation(string conversationId, DateTime newTimeStamp)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException("conversationId cannot be null or empty");
            }

            var idFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, conversationId);
            var idQuery = new TableQuery<UsersTableEntity>().Where(idFilter);
            var conversations = await table.ExecuteQuery(idQuery);

            if (conversations.Results.Count == 0)
            {
                throw new ConversationNotFoundException("Could not find the requested conversation");
            }

            if (conversations.Results.Count != 2)
            {
                throw new StorageException("There should be exactly two entities");
            }


            var firstTableBatchOperation = await UpdateEntitiesForUser(conversations.Results[0], newTimeStamp);
            var secondTableBatchOperation = await UpdateEntitiesForUser(conversations.Results[1], newTimeStamp);

            try
            {
                await Task.WhenAll(table.ExecuteBatchAsync(firstTableBatchOperation),
                    table.ExecuteBatchAsync(secondTableBatchOperation));
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not write to azure table", e);
            }
        }

        public async Task<UsersTableEntity> retrieveEntity(string partitionKey, string rowKey)
        {
            var partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var rowKeyFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);
            var fullFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);

            var query = new TableQuery<UsersTableEntity>().Where(fullFilter);
            var result = await table.ExecuteQuery(query);

            if (result.Results.Count == 0)
            {
                throw new ConversationNotFoundException("Could not find old conversation");
            }

            return result.Results[0];
        }

        private void validateConversation(Conversation conversation)
        {
            if (conversation == null)
            {
                throw new ArgumentNullException(nameof(conversation) + " cannot be null");
            }

            if (string.IsNullOrWhiteSpace(conversation.Id))
            {
                throw new ArgumentNullException(nameof(conversation.Id) + " cannot be null");
            }

            if (conversation.LastModifiedDateUtc == null)
            {
                throw new ArgumentNullException(nameof(conversation.LastModifiedDateUtc) + " cannot be null");
            }

            if (conversation.Participants == null || conversation.Participants.Length == 0)
            {
                throw new ArgumentNullException(nameof(conversation.Participants) + " cannot be null or empty");
            }

            foreach (var participant in conversation.Participants)
            {
                if (string.IsNullOrWhiteSpace(participant))
                {
                    throw new ArgumentNullException(nameof(participant) + " cannot be null");
                }
            }
        }

        private string datetimeToTicks(DateTime datetime)
        {
            return rowkeyTsPrefix + string.Format("{0:D19}", DateTime.MaxValue.Ticks - datetime.Ticks);
        }

        private DateTime ticksToDateTime(string ticks)
        {
            ticks = ticks.Replace("ticks_", "");
            return new DateTime(DateTime.MaxValue.Ticks - Convert.ToInt64(ticks));
        }
    }
}