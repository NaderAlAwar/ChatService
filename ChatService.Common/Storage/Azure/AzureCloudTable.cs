﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public class AzureCloudTable : ICloudTable
    {
        private readonly CloudTable table;

        public AzureCloudTable(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(tableName);
        }

        public Task CreateIfNotExistsAsync()
        {
            return table.CreateIfNotExistsAsync();
        }

        public Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return table.ExecuteAsync(operation);
        }

        public Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation batchOperation)
        {
            return table.ExecuteBatchAsync(batchOperation);
        }

        public Task<TableQuerySegment<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new()
        {
            return table.ExecuteQuerySegmentedAsync(query, token);
        }

        public Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query) where T : ITableEntity, new()
        {
            return ExecuteQuerySegmentedAsync(query, null);
        }
    }
}