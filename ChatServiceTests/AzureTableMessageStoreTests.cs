using System;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatServiceTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class AzureTableMessageStoreTests
    {
        private Mock<ICloudTable> tableMock;
        private AzureTableMessageStore store;

        private readonly Message message = new Message("Hello", "Elie", DateTime.UtcNow);

        [TestInitialize]
        public void TestInitialize()
        {
            tableMock = new Mock<ICloudTable>();
            store = new AzureTableMessageStore(tableMock.Object);

            tableMock.Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            tableMock.Setup(m => m.ExecuteQuery(It.IsAny<TableQuery<MessageTableEntity>>(), It.IsAny<TableContinuationToken>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task ListMessages_StorageIsUnavailable()
        {
            await store.ListMessages("I am a conversationId");
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddMessage_StorageIsUnavailable()
        {
            await store.AddMessage("I am a conversationId", message);
        }

    }
}