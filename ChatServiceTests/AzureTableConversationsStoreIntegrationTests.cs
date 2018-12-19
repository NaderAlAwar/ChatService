using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatServiceTests.Utils;
using ChatService.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{

    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableConversationsStoreIntegrationTests
    {
        private AzureTableConversationsStore store;
        private static string ConnectionString { get; set; }
        private static AzureStorageSettings azureStorageSettings;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            IConfiguration configuration = TestUtils.InitConfiguration();
            IConfiguration storageConfiguration = configuration.GetSection(nameof(AzureStorageSettings));
            AzureStorageSettings azureStorageSettings = new AzureStorageSettings();
            storageConfiguration.Bind(azureStorageSettings);
            ConnectionString = azureStorageSettings.ConnectionString;
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            var messagesTable = new AzureCloudTable(ConnectionString, "MessagesTable");
            await messagesTable.CreateIfNotExistsAsync();
            var messageStore = new AzureTableMessagesStore(messagesTable);

            var conversationsTable = new AzureCloudTable(ConnectionString, "ConversationsTable");
            await conversationsTable.CreateIfNotExistsAsync();
            store = new AzureTableConversationsStore(conversationsTable, messageStore);
        }

        [TestMethod]
        [ExpectedException(typeof(ConversationNotFoundException))]
        public async Task GetConversation_ConversationDoesNotExist()
        {
            await store.GetConversation(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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
                await store.AddMessage(conversationId, RandomString(), message);
            }

            List<Message> listedMessages = (await store.ListMessages(conversationId, "", "", 50)).Messages.ToList();
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
                await store.AddMessage(conversationId, RandomString(), message);
            }

            List<Message> listedMessages = (await store.ListMessages(conversationId, "", "", 50)).Messages.ToList();
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

            var userBConversations = (await store.ListConversations(userB, "", "", 50)).Conversations.ToList();
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
            await store.AddMessage(conversations[0].Id, RandomString(), message);

            var userAConversations = (await store.ListConversations(userA, "", "", 50)).Conversations.ToList();
            Assert.AreEqual(3, userAConversations.Count());
            Assert.AreEqual(conversations[0].Id, userAConversations[0].Id);
            Assert.AreEqual(message.UtcTime, userAConversations[0].LastModifiedDateUtc);

            var userBConversations = (await store.ListConversations(userB, "", "", 50)).Conversations.ToList();
            Assert.AreEqual(2, userBConversations.Count());
            Assert.AreEqual(conversations[0].Id, userBConversations[0].Id);
            Assert.AreEqual(message.UtcTime, userBConversations[0].LastModifiedDateUtc);
        }

        [TestMethod]
        public async Task ConversationsPaging()
        {
            string userA = RandomString();
            string userB = RandomString();
            string userC = RandomString();
            string userD = RandomString();

            var dateTime = DateTime.UtcNow;

            var conversations = new[]
            {
                new Conversation(RandomString(), new [] { userA, userB }, dateTime.AddSeconds(1)),
                new Conversation(RandomString(), new [] { userA, userC }, dateTime.AddSeconds(2)),
                new Conversation(RandomString(), new [] { userA, userD }, dateTime.AddSeconds(3)),
            };

            foreach (Conversation conversation in conversations)
            {
                await store.AddConversation(conversation);
            }

            List<Conversation> userAConversations = (await store.ListConversations(userA, OrderedConversationEntity.ToRowKey(dateTime.AddSeconds(0)), "", 2)).Conversations.ToList();
            Assert.AreEqual(2, userAConversations.Count);
            Assert.AreEqual(userD, userAConversations[0].Participants[1]);
            Assert.AreEqual(userC, userAConversations[1].Participants[1]);

            userAConversations = (await store.ListConversations(userA, OrderedConversationEntity.ToRowKey(dateTime.AddSeconds(1)), "", 2)).Conversations.ToList();

            Assert.AreEqual(2, userAConversations.Count);
            Assert.AreEqual(userD, userAConversations[0].Participants[1]);
            Assert.AreEqual(userC, userAConversations[1].Participants[1]);

            userAConversations = (await store.ListConversations(userA, OrderedConversationEntity.ToRowKey(dateTime.AddSeconds(3)), "", 2)).Conversations.ToList();
            Assert.AreEqual(0, userAConversations.Count);

            userAConversations = (await store.ListConversations(userA, "", OrderedConversationEntity.ToRowKey(dateTime.AddSeconds(3)), 2)).Conversations.ToList();
            Assert.AreEqual(2, userAConversations.Count);
            Assert.AreEqual(userC, userAConversations[0].Participants[1]);
            Assert.AreEqual(userB, userAConversations[1].Participants[1]);

            userAConversations = (await store.ListConversations(userA, "", OrderedConversationEntity.ToRowKey(dateTime.AddSeconds(1)), 2)).Conversations.ToList();
            Assert.AreEqual(0, userAConversations.Count);
        }

        [TestMethod]
        public async Task MessagesPaging()
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
                await store.AddMessage(conversationId, RandomString(), message);
            }

            List<Message> returnedMessages = (await store.ListMessages(conversationId, DateTimeUtils.FromDateTimeToInvertedString(dateTime), "", 2)).Messages.ToList();
            Assert.AreEqual(2, returnedMessages.Count);
            Assert.AreEqual("Cool! Are you taking EECE503E?", returnedMessages[0].Text);
            Assert.AreEqual("Writing some code!", returnedMessages[1].Text);

            returnedMessages = (await store.ListMessages(conversationId, DateTimeUtils.FromDateTimeToInvertedString(dateTime.AddSeconds(4)), "", 2)).Messages.ToList();
            Assert.AreEqual(0, returnedMessages.Count);

            returnedMessages = (await store.ListMessages(conversationId, "", DateTimeUtils.FromDateTimeToInvertedString(dateTime.AddSeconds(5)), 2)).Messages.ToList();
            Assert.AreEqual(2, returnedMessages.Count);
            Assert.AreEqual("Cool! Are you taking EECE503E?", returnedMessages[0].Text);
            Assert.AreEqual("Writing some code!", returnedMessages[1].Text);

            returnedMessages = (await store.ListMessages(conversationId, "", DateTimeUtils.FromDateTimeToInvertedString(dateTime.AddSeconds(1)), 2)).Messages.ToList();
            Assert.AreEqual(0, returnedMessages.Count);
        }
    }
}