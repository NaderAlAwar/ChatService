using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public interface ICloudTable
    {
        Task CreateIfNotExistsAsync();
        Task<TableResult> ExecuteAsync(TableOperation operation);
        Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation operation);
        Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query)
            where T: ITableEntity, new();
        Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query, TableContinuationToken token) 
        where T : ITableEntity, new();

    }
}