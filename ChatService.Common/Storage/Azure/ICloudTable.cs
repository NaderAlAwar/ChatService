using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ChatService.Storage.Azure
{
    public interface ICloudTable
    {
        Task CreateIfNotExistsAsync();
        Task<TableResult> ExecuteAsync(TableOperation operation);
    }
}