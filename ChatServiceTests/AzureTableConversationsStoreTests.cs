﻿using System;
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
    public class AzureTableConversationsStoreTests
    {
        private readonly Mock<ICloudTable> tableMock = new Mock<ICloudTable>();
        private readonly Mock<IMessagesStore> messageStoreMock = new Mock<IMessagesStore>();
        private AzureTableConversationsStore store;


        [TestInitialize]
        public void TestInitialize()
        {
            store = new AzureTableConversationsStore(tableMock.Object, messageStoreMock.Object);
            tableMock.Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            tableMock.Setup(m => m.ExecuteQuery(It.IsAny<TableQuery<OrderedConversationEntity>>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
            tableMock.Setup(m => m.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ListConversations_InvalidUsername(string username)
        {
            await store.ListConversations(username, "", "", 50);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task ListConversations_StorageIsUnavailable()
        {
            await store.ListConversations("foo", "", "", 50);
        }

        [TestMethod]
        [DataRow(null, "foo,bar")]
        [DataRow("", "foo,bar")]
        [DataRow("foo", "bar")]
        [DataRow("foo", "")]
        [DataRow("foo", "foo,")]
        [DataRow("foo", ",bar")]
        [DataRow("foo", ",")]
        [DataRow("", "foo,bar,toto")]
        public async Task AddConversation_InvalidConversation(string id, string participants)
        {
            try
            {
                Conversation conversation = new Conversation(id, participants.Split(','), DateTime.UtcNow);
                await store.AddConversation(conversation);

                Assert.Fail($"Expected {nameof(ArgumentException)} was not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        [DataRow(null, "foo")]
        [DataRow("", "foo")]
        [DataRow("foo", null)]
        [DataRow("foo", "")]
        public async Task GetConversation_InvalidConversation(string username, string conversationId)
        {
            try
            {
                await store.GetConversation(username, conversationId);

                Assert.Fail($"Expected {nameof(ArgumentException)} was not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddConversation_NullConversation()
        {
            await store.AddConversation(null);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddConversation_StorageIsUnavailable()
        {
            await store.AddConversation(new Conversation("foo", new []{"foo", "bar"}, DateTime.UtcNow));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddMessage_NullMessage()
        {
            await store.AddMessage("foo", "bar", null);
        }

        [DataRow("", "messageId", "bla bla", "foo")]
        [DataRow(null, "messageId", "bla bla", "foo")]
        [DataRow("conversationId", "", "bla bla", "foo")]
        [DataRow("conversationId", null, "bla bla", "foo")]
        [DataRow("conversationId", "messageId", "bla bla", null)]
        [DataRow("conversationId", "messageId", "bla bla", "")]
        [TestMethod]
        public async Task AddMessage_InvalidMessage(string conversationId, string messageId, string text, string sender)
        {
            try
            {
                var message = new Message(text, sender, DateTime.UtcNow);
                await store.AddMessage(conversationId, messageId, message);

                Assert.Fail($"Expected {nameof(ArgumentException)} was not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddMessage_ConversationStorageIsUnavailable()
        {
            await store.AddMessage("conversationId", "messageId", new Message("bla bla", "foo", DateTime.UtcNow));
        }
    }
}