using System;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableUserStoreTests
    {
        private Mock<ICloudTable> tableMock;
        private AzureTableUserStore store;

        private readonly Conversation testConversation = new Conversation("foo_bar", new []{"foo","bar"}, DateTime.Now);

        [TestInitialize]
        public void TestInitialize()
        {
            tableMock = new Mock<ICloudTable>();
            store = new AzureTableUserStore(tableMock.Object);



            tableMock.Setup(m => m.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task ListConversations_StorageIsUnavailable()
        {
            tableMock.Setup(m => m.ExecuteQuery(It.IsAny<TableQuery<UsersTableEntity>>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            await store.ListConversations("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddConversation_StorageIsUnavailable()
        {
            await store.AddConversation(testConversation);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task UpdateConversation_StorageIsUnavailable()
        {
            await store.AddConversation(testConversation);
            await store.UpdateConversation("foo", DateTime.Now);
        }
    }
}