using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{

    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableConversationsStoreIntegrationTests
    {
        private const string connectionString = "UseDevelopmentStorage=true";
        private static AzureStorageEmulatorProxy emulator;
        private AzureTableConversationsStore store;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            emulator = new AzureStorageEmulatorProxy();
            emulator.StartEmulator();
            emulator.ClearAll();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            emulator.StopEmulator();
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            emulator = new AzureStorageEmulatorProxy();
            emulator.StartEmulator();
            var messagesTable = new AzureCloudTable(connectionString, "MessagesTable");
            await messagesTable.CreateIfNotExistsAsync();
            var messageStore = new AzureTableMessagesStore(messagesTable);

            var conversationsTable = new AzureCloudTable(connectionString, "ConversationsTable");
            await conversationsTable.CreateIfNotExistsAsync();
            store = new AzureTableConversationsStore(conversationsTable, messageStore);
        }

        [TestMethod]
        public async Task AddListMessages()
        {
            string conversationId = RandomString();
            var dateTime = DateTime.UtcNow;

            string userA = RandomString();
            string userB = RandomString();

            await store.AddConversation(new Conversation(conversationId, new[] { userA, userB }, dateTime));

            var messages = new[]
            {
                new Message("Hola what's up?", userA, dateTime.AddSeconds(1)),
                new Message("Not much you?", userB, dateTime.AddSeconds(2)),
                new Message("Writing some code!", userA, dateTime.AddSeconds(3)),
                new Message("Cool! Are you taking EECE503E?", userB, dateTime.AddSeconds(4))
            };

            foreach (var message in messages)
            {
                await store.AddMessage(conversationId, message);
            }

            List<Message> listedMessages = (await store.ListMessages(conversationId)).ToList();
            var reversedMessages = messages.Reverse().ToList();
            CollectionAssert.AreEqual(reversedMessages, listedMessages);
        }

        private static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }

        [TestMethod]
        public async Task MessagesAreOrderedByDate()
        {
            string conversationId = RandomString();
            var dateTime = DateTime.UtcNow;

            string userA = RandomString();
            string userB = RandomString();

            await store.AddConversation(new Conversation(conversationId, new[] { userA, userB }, dateTime));

            var messages = new[]
            {
                new Message("Not much you?", userA, dateTime.Subtract(TimeSpan.FromSeconds(2))),
                new Message("Hola what's up?", userB, dateTime.Subtract(TimeSpan.FromSeconds(5))) // this message is older
            };

            foreach (var message in messages)
            {
                await store.AddMessage(conversationId, message);
            }

            List<Message> listedMessages = (await store.ListMessages(conversationId)).ToList();
            CollectionAssert.AreEquivalent(messages.ToList(), listedMessages); // messages are ordered from newer to older
        }

        [TestMethod]
        public async Task AddListConversations()
        {
            string userA = RandomString();
            string userB = RandomString();
            string userC = RandomString();

            var dateTime = DateTime.UtcNow;

            var conversations = new[]
            {
                new Conversation(RandomString(), new [] { userA, userB }, dateTime.AddSeconds(1)),
                new Conversation(RandomString(), new [] { userA, userC }, dateTime.AddSeconds(2)),
                new Conversation(RandomString(), new [] { userB, userA }, dateTime.AddSeconds(3)),
            };

            foreach (Conversation conversation in conversations)
            {
                await store.AddConversation(conversation);
            }

            var userBConversations = (await store.ListConversations(userB)).ToList();
            Assert.AreEqual(2, userBConversations.Count());
            Assert.AreEqual(conversations[2], userBConversations[0]);
            Assert.AreEqual(conversations[0], userBConversations[1]);
        }

        [TestMethod]
        public async Task AddingAMessageUpdatesConversationDate()
        {
            string userA = RandomString();
            string userB = RandomString();
            string userC = RandomString();

            var dateTime = DateTime.UtcNow;

            var conversations = new[]
            {
                new Conversation(RandomString(), new [] { userA, userB }, dateTime.AddSeconds(1)),
                new Conversation(RandomString(), new [] { userA, userC }, dateTime.AddSeconds(2)),
                new Conversation(RandomString(), new [] { userB, userA }, dateTime.AddSeconds(3)),
            };

            foreach (Conversation conversation in conversations)
            {
                await store.AddConversation(conversation);
            }

            // this conversation should become the most recent because we added a message to it
            Message message = new Message("bla bla", userA, dateTime.AddSeconds(4));
            await store.AddMessage(conversations[0].Id, message);

            var userAConversations = (await store.ListConversations(userA)).ToList();
            Assert.AreEqual(3, userAConversations.Count());
            Assert.AreEqual(conversations[0].Id, userAConversations[0].Id);
            Assert.AreEqual(message.UtcTime, userAConversations[0].LastModifiedDateUtc);

            var userBConversations = (await store.ListConversations(userB)).ToList();
            Assert.AreEqual(2, userBConversations.Count());
            Assert.AreEqual(conversations[0].Id, userBConversations[0].Id);
            Assert.AreEqual(message.UtcTime, userBConversations[0].LastModifiedDateUtc);
        }
    }
}