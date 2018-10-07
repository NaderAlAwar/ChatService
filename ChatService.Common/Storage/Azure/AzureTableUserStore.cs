using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureTableUserStore : IConversationsStore
    {
        private readonly ICloudTable table;

        public AzureTableUserStore(ICloudTable cloudTable)
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
                var newConversation = new Conversation(conversation.conversationId, new[] { username, conversation.recipient }, conversation.TicksToDateTime());
                result.Add(newConversation);
            }
            result.Reverse();
            return result;
        }

        public async Task AddConversation(Conversation conversation)
        {
            validateConversation(conversation);
            var firstIdEntity = new UsersTableEntity()
            {
                PartitionKey = conversation.Participants[0],
                RowKey = conversation.Id,
                dateTime = conversation.LastModifiedDateUtc.Ticks.ToString(),
                recipient = conversation.Participants[1]
            };

            var secondIdEntity = new UsersTableEntity()
            {
                PartitionKey = conversation.Participants[1],
                RowKey = conversation.Id,
                dateTime = conversation.LastModifiedDateUtc.Ticks.ToString(),
                recipient = conversation.Participants[0]
            };

            var firstTsEntity = new UsersTableEntity()
            {
                PartitionKey = conversation.Participants[0],
                RowKey = "ticks_" + conversation.LastModifiedDateUtc.Ticks.ToString(),
                conversationId = conversation.Id,
                recipient = conversation.Participants[1]
            };

            var secondTsEntity = new UsersTableEntity()
            {
                PartitionKey = conversation.Participants[1],
                RowKey = "ticks_" + conversation.LastModifiedDateUtc.Ticks.ToString(),
                conversationId = conversation.Id,
                recipient = conversation.Participants[0]
            };

            var firstTableBatchOperation = new TableBatchOperation();
            var secondTableBatchOperation = new TableBatchOperation();

            firstTableBatchOperation.Insert(firstIdEntity);
            firstTableBatchOperation.Insert(firstTsEntity);
            secondTableBatchOperation.Insert(secondIdEntity);
            secondTableBatchOperation.Insert(secondTsEntity);

            try
            {
                await table.ExecuteBatchAsync(firstTableBatchOperation);
                await table.ExecuteBatchAsync(secondTableBatchOperation);
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
                throw new StorageException("There should be exactly two conversations");
            }

            string oldTimeStamp = "ticks_" + conversations.Results[0].dateTime; // should be the same for both conversations
            string newTimeStampInTicks = "ticks_" + newTimeStamp.Ticks;

            string userOne = conversations.Results[0].recipient;
            string userTwo = conversations.Results[1].recipient;

            conversations.Results[0].dateTime = newTimeStampInTicks;    // update old entities
            conversations.Results[1].dateTime = newTimeStampInTicks;

            var oldEntityOne = await retrieveEntity(userOne, oldTimeStamp);   // these will be deleted
            var oldEntityTwo = await retrieveEntity(userTwo, oldTimeStamp);

            var newEntityOne = new UsersTableEntity // these will be inserted
            {
                PartitionKey = userOne, RowKey = newTimeStampInTicks, conversationId = conversationId
            };

            var newEntityTwo = new UsersTableEntity()
            {
                PartitionKey = userTwo, RowKey = newTimeStampInTicks, conversationId = conversationId
            };

            var firstTableBatchOperation = new TableBatchOperation();
            var secondTableBatchOperation = new TableBatchOperation();
            
            firstTableBatchOperation.Replace(conversations.Results[1]);
            secondTableBatchOperation.Replace(conversations.Results[0]);
            firstTableBatchOperation.Delete(oldEntityOne);
            secondTableBatchOperation.Delete(oldEntityTwo);
            firstTableBatchOperation.Insert(newEntityOne);
            secondTableBatchOperation.Insert(newEntityTwo);
            
            try
            {
                await table.ExecuteBatchAsync(firstTableBatchOperation);
                await table.ExecuteBatchAsync(secondTableBatchOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Could not write to azure table", e);
            }
        }

        //        public async Task UpdateConversation(Conversation conversation)
        //        {
        //            validateConversation(conversation);
        //
        //            UsersTableEntity userOneEntity = await retrieveEntity(conversation.Participants[0], conversation.Id);
        //            UsersTableEntity userTwoEntity = await retrieveEntity(conversation.Participants[1], conversation.Id);
        //
        //            var userOneFilter =
        //                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversation.Participants[0]);
        //            var userTwoFilter =
        //                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversation.Participants[1]);
        //            var tsFilter =
        //                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "ticks_" + userOneEntity.dateTime);   // should be the same for both entities
        //
        //            var queryOne = new TableQuery<UsersTableEntity>().Where(
        //                TableQuery.CombineFilters(userOneFilter, TableOperators.And, tsFilter));
        //            var queryTwo = new TableQuery<UsersTableEntity>().Where(
        //                TableQuery.CombineFilters(userTwoFilter, TableOperators.And, tsFilter));
        //
        //            var resultOne = await table.ExecuteQuery(queryOne);
        //            var resultTwo = await table.ExecuteQuery(queryTwo);
        //
        //            if (resultOne.Results.Count == 0 || resultTwo.Results.Count == 0)
        //            {
        //                throw new ConversationNotFoundException("Could not find the requested conversation");
        //            }
        //
        //            UsersTableEntity userOneTsEntity = resultOne.Results[0];
        //            UsersTableEntity userTwoTsEntity = resultTwo.Results[0];
        //
        //            string currentTime = "ticks_" + conversation.LastModifiedDateUtc.Ticks.ToString();
        //            userOneEntity.dateTime = currentTime;
        //            userTwoEntity.dateTime = currentTime;
        //
        //            var firstTableBatchOperation = new TableBatchOperation();
        //            var secondTableBatchOperation = new TableBatchOperation();
        //
        //            firstTableBatchOperation.Replace(userOneEntity);
        //            secondTableBatchOperation.Replace(userTwoEntity);
        //            firstTableBatchOperation.Delete(userOneTsEntity);
        //            secondTableBatchOperation.Delete(userTwoTsEntity);
        //
        //            var newTsEntityOne = new UsersTableEntity
        //            {
        //                PartitionKey = conversation.Participants[0],
        //                RowKey = currentTime,
        //                conversationId = conversation.Id
        //            };
        //            var newTsEntityTwo = new UsersTableEntity
        //            {
        //                PartitionKey = conversation.Participants[1],
        //                RowKey = currentTime,
        //                conversationId = conversation.Id
        //            };
        //
        //            firstTableBatchOperation.Insert(newTsEntityOne);
        //            secondTableBatchOperation.Insert(newTsEntityTwo);
        //
        //            try
        //            {
        //                await table.ExecuteBatchAsync(firstTableBatchOperation);
        //                await table.ExecuteBatchAsync(secondTableBatchOperation);
        //            }
        //            catch (StorageException e)
        //            {
        //                throw new StorageErrorException("Could not write to azure table", e);
        //            }
        //        }

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
    }
}