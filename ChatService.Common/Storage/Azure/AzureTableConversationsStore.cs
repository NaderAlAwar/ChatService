using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureTableConversationsStore : IConversationsStore
    {
        private readonly ICloudTable table;
        private readonly IMessagesStore messagesStore;

        private const char ParticipantsSeparator = ',';

        public AzureTableConversationsStore(ICloudTable cloudTable, IMessagesStore messagesStore)
        {
            this.table = cloudTable;
            this.messagesStore = messagesStore;
        }

        public async Task<IEnumerable<Conversation>> ListConversations(string username, string startCt, string endCt, int limit)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            string high = string.IsNullOrEmpty(startCt) ? OrderedConversationEntity.MaxRowKey : startCt;
            string low = string.IsNullOrEmpty(endCt) ? OrderedConversationEntity.MinRowKey : endCt;

            TableQuery<OrderedConversationEntity> query = new TableQuery<OrderedConversationEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, username),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan,
                            low),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan,
                            high)
                    )
                )
            );

            query.TakeCount = limit;

            try
            {
                var results = await table.ExecuteQuery(query);
                List<OrderedConversationEntity> entities = results.Results;
                return entities.Select(entity => new Conversation(entity.ConversationId, 
                    entity.Participants.Split(ParticipantsSeparator), entity.GetLastModifiedDateTimeUtc()));
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Failed to list messages", e);
            }
        }

        public async Task AddConversation(Conversation conversation)
        {
            if (conversation == null)
            {
                throw new ArgumentNullException(nameof(conversation));
            }

            if (string.IsNullOrWhiteSpace(conversation.Id))
            {
                throw new ArgumentException("Conversation id is missing");
            }

            if (conversation.Participants == null || conversation.Participants.Length != 2)
            {
                throw new ArgumentException("A conversation should have at least two participants");
            }

            if (string.IsNullOrWhiteSpace(conversation.Participants[0]) || string.IsNullOrWhiteSpace(conversation.Participants[1]))
            {
                throw new ArgumentException("A valid username should be specified for each participant");
            }

            await Task.WhenAll(
                AddConversationForUser(conversation.Participants[0], conversation),
                AddConversationForUser(conversation.Participants[1], conversation)
            );
        }

        public Task<IEnumerable<Message>> ListMessages(string conversationId)
        {
            return messagesStore.ListMessages(conversationId);
        }

        public async Task AddMessage(string conversationId, Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            if (string.IsNullOrWhiteSpace(message.SenderUsername))
            {
                throw new ArgumentNullException(nameof(message.SenderUsername));
            }

            // Add the message first
            await messagesStore.AddMessage(conversationId, message);

            // Then update the conversations modified date for each user
            ConversationEntity conversationEntity = await RetrieveConversationEntity(message.SenderUsername, conversationId);

            var tasks = new List<Task>();
            foreach (var participant in conversationEntity.Participants.Split(ParticipantsSeparator))
            {
                tasks.Add(UpdateConversationModifiedDateForUser(participant, conversationId, message));
            }
            await Task.WhenAll(tasks);
        }

        private async Task UpdateConversationModifiedDateForUser(string username, string conversationId, Message message)
        {
            ConversationEntity conversationEntity = await RetrieveConversationEntity(username, conversationId);
            var newOrderedConversationEntity = new OrderedConversationEntity(
                username, conversationId, conversationEntity.GetParticipants(), message.UtcTime);
            string oldTicksRowKey = conversationEntity.TicksRowKey;
            string newTicksRowKey = newOrderedConversationEntity.RowKey;
            conversationEntity.TicksRowKey = newTicksRowKey;

            TableBatchOperation batchOperation = new TableBatchOperation
            {
                TableOperation.Replace(conversationEntity), // will fail if entity has changed since we retrieved it (ETAG)
                TableOperation.Delete(new TableEntity(partitionKey: username, rowKey: oldTicksRowKey) { ETag = "*"}), // delete old conversation order entity
                TableOperation.Insert(newOrderedConversationEntity) // will fail if another entity with the same ticks exists
            };

            try
            {
                await table.ExecuteBatchAsync(batchOperation);
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Failed to update conversation modified time", e);
            }
        }

        private async Task<ConversationEntity> RetrieveConversationEntity(string username, string conversationId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<ConversationEntity>(partitionKey: username, rowkey: ConversationEntity.ToRowKey(conversationId));

            try
            {
                TableResult tableResult = await table.ExecuteAsync(retrieveOperation);
                var entity = (ConversationEntity)tableResult.Result;

                if (entity == null)
                {
                    throw new ConversationNotFoundException($"Could not find a conversation with id {conversationId}");
                }

                return entity;
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not retrieve conversation {conversationId} from storage", e);
            }
        }

        private async Task AddConversationForUser(string username, Conversation conversation)
        {
            var conversationOrderEntity = new OrderedConversationEntity(username, conversation.Id, conversation.Participants, conversation.LastModifiedDateUtc);
            var conversationEntity = new ConversationEntity(username, conversation.Id, conversation.Participants,
                conversationOrderEntity.RowKey);

            TableBatchOperation batchInsert = new TableBatchOperation
            {
                TableOperation.Insert(conversationEntity),
                TableOperation.Insert(conversationOrderEntity)
            };

            try
            {
                await table.ExecuteBatchAsync(batchInsert);
            }
            catch (StorageException e)
            {
                // Do nothing if the conversation already exists.
                // The client should list all conversations periodically or based on events and will get
                // the latest list of conversations.
                if (e.RequestInformation.HttpStatusCode != 409)
                {
                    throw new StorageErrorException("Failed to reach storage", e);
                }
            }
        }
    }
}