using System.Collections;
using System.Collections.Generic;
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

        public async Task CreateIfNotExistsAsync()
        {
            await table.CreateIfNotExistsAsync();
        }

        public Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return table.ExecuteAsync(operation);
        }

        public async Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation operation)
        {
            return await table.ExecuteBatchAsync(operation);
        }

        public async Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query)
        where T : ITableEntity, new()
        {
            return await table.ExecuteQuerySegmentedAsync(query, null);
        }

        public Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query, TableContinuationToken token) 
        where T : ITableEntity, new()
        {
            return table.ExecuteQuerySegmentedAsync<T>(query, token);
        }
    }
}