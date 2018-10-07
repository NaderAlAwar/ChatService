using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureTableMessageStore : IMessageStore
    {
        private readonly ICloudTable table;
        private readonly IConversationsStore conversationsStore;

        public AzureTableMessageStore(ICloudTable cloudTable, IConversationsStore conversationsStore)
        {
            this.table = cloudTable;
            this.conversationsStore = conversationsStore;
        }

        public async Task<IEnumerable<Message>> ListMessages(string conversationId)
        {
            var messageEntities = await RetrieveMessagesForConversation(conversationId);

            var messageList = new List<Message>();

            foreach (MessageTableEntity entity in messageEntities)
            {
                Message message = new Message(entity.Text, entity.SenderUsername, new DateTime(-1 * (Convert.ToInt64(entity.RowKey) - DateTime.MaxValue.Ticks)));
                messageList.Add(message);
            }

            return messageList;
        }

        public async Task AddMessage(string conversationId, Message message)
        {
            ValidateMessage(message);

            var messageTableEntity = new MessageTableEntity
            {
                PartitionKey = conversationId,
                RowKey = GetInvertedTimeInTicksAsString(message.UtcTime),
                SenderUsername = message.SenderUsername,
                Text = message.Text,
            };

            TableOperation insertOperation = TableOperation.Insert(messageTableEntity);
            try
            {
                await table.ExecuteAsync(insertOperation);
                await conversationsStore.UpdateConversation(conversationId, message.UtcTime);
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 409) // not found
                {
                    throw new DuplicateMessageException($"Message from conversation with ID {conversationId} and timestap {message.UtcTime} already exists");
                }
                throw new StorageErrorException("Could not write to Azure Table", e);
            }
        }

        public async Task<bool> TryDelete(string conversationId, DateTime utcTime)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }

            try
            {
                var entity = await RetrieveMessage(conversationId, utcTime);

                TableOperation deleteOperation = TableOperation.Delete(entity);
                await table.ExecuteAsync(deleteOperation);
                return true;
            }
            catch (MessageNotFoundException)
            {
                return false;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == 412) // precondition failed
                {
                    throw new StorageConflictException("Optimistic concurrency failed");
                }
                throw new StorageErrorException($"Could not delete message from storage, conversationId = {conversationId}, utcTime = {utcTime.ToBinary().ToString()}", e);
            }
        }

        private async Task<MessageTableEntity> RetrieveMessage(string conversationId, DateTime utcTime)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId));
            }
            if (utcTime == null)
            {
                throw new ArgumentNullException(nameof(utcTime));
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<MessageTableEntity>(partitionKey: conversationId, rowkey: GetInvertedTimeInTicksAsString(utcTime));

            try
            {
                TableResult tableResult = await table.ExecuteAsync(retrieveOperation);
                var entity = (MessageTableEntity)tableResult.Result;

                if (entity == null)
                {
                    throw new MessageNotFoundException($"Could not find a message in conversation {conversationId} and timestamp {utcTime.ToBinary().ToString()}");
                }

                return entity;
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not retrieve message in conversation {conversationId} and timestamp {utcTime} from storage", e);
            }
        }

        private async Task<IEnumerable<MessageTableEntity>> RetrieveMessagesForConversation(string conversationId)
        {
            TableQuery<MessageTableEntity> listMessagesQuery = new TableQuery<MessageTableEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversationId)
            );

            try
            {
                var messageEntitiesList = new List<MessageTableEntity>();
                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<MessageTableEntity> segment = await table.ExecuteQuery<MessageTableEntity>(listMessagesQuery, token);
                    messageEntitiesList.AddRange(segment.Results);
                } while (token != null);

                return messageEntitiesList;
            }
            catch (StorageException e)
            {
                throw new StorageErrorException($"Could not retrieve messages for conversation with ID {conversationId} from storage", e);
            }
        }

        private string GetInvertedTimeInTicksAsString(DateTime utcTime) {
            return string.Format("{0:D19}", DateTime.MaxValue.Ticks - utcTime.Ticks);
        }

        private void ValidateMessage(Message message)
        {
            MessageUtils.Validate(message);
        }

    }
}
