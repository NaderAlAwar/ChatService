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
    [TestCategory("UnitTests")]
    public class AzureTableMessagesStoreTests
    {
        private readonly Mock<ICloudTable> tableMock = new Mock<ICloudTable>();
        private AzureTableMessagesStore store;

        [TestInitialize]
        public void TestInitialize()
        {
            store = new AzureTableMessagesStore(tableMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task ListMessages_StorageUnavailable()
        {
            tableMock.Setup(m => m.ExecuteQuery(It.IsAny<TableQuery<MessageEntity>>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            await store.ListMessages("conversationId", "", "", 50);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddMessage_StorageUnavailable()
        {
            tableMock.Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            await store.AddMessage("conversationId", new Message("text", "username", DateTime.UtcNow));
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddMessage_Conflict()
        {
            tableMock.Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 409 }, "Message already exists", null));
            await store.AddMessage("conversationId", new Message("text", "username", DateTime.UtcNow));
        }

        [DataRow(null)]
        [DataRow("")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddMessage_InvalidSenderUsername(string username)
        {
            await store.AddMessage(Guid.NewGuid().ToString(),
                new Message(Guid.NewGuid().ToString(), username, DateTime.Now));
        }
    }
}