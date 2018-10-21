﻿using System;
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

        public async Task<IEnumerable<Message>> ListMessages(string conversationId)
        {
            TableQuery<MessageEntity> query = new TableQuery<MessageEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, conversationId));
            query.TakeCount = 50;

            try
            {
                var results = await table.ExecuteQuery(query);
                List<MessageEntity> entities = results.Results;
                return entities.Select(entity =>
                {
                    long ticks = long.Parse(entity.RowKey);
                    ticks = DateTimeUtils.InvertTicks(ticks);
                    var utcTime = new DateTime(ticks);
                    return new Message(entity.Text, entity.SenderUsername, utcTime);
                });
            }
            catch (StorageException e)
            {
                throw new StorageErrorException("Failed to list messages", e);
            }
        }

        public async Task AddMessage(string conversationId, Message message)
        {
            if (string.IsNullOrWhiteSpace(message.SenderUsername))
            {
                throw new ArgumentNullException(nameof(message.SenderUsername));
            }

            MessageEntity entity = new MessageEntity
            {
                PartitionKey = conversationId,
                RowKey = DateTimeUtils.InvertTicks(message.UtcTime.Ticks).ToString("d19"),
                SenderUsername = message.SenderUsername,
                Text = message.Text
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
    }
}