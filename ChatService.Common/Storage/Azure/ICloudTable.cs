using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public interface ICloudTable
    {
        Task CreateIfNotExistsAsync();
        Task<TableResult> ExecuteAsync(TableOperation operation);
<<<<<<< 32085f2f7d2ebf48b3f2b09b4aea4cba5e185473
        Task<IList<TableResult>> ExecuteBatchAsync(TableBatchOperation operation);
        Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query)
            where T: ITableEntity, new();
=======
        Task<TableQuerySegment<T>> ExecuteQuery<T>(TableQuery<T> query, TableContinuationToken token) 
        where T : ITableEntity, new();

>>>>>>> Add the needed functionality to allow the saving of messages in Azure
    }
}