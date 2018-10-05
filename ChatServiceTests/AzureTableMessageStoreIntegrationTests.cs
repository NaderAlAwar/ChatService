using System;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatServiceTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableMessageStoreIntegrationTests
    {
        private static string ConnectionString { get; set; }

        private AzureTableMessageStore store;

        private string conversationId = Guid.NewGuid().ToString();

        Message testMessage = new Message("Hello", "Elie", DateTime.UtcNow);

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            IConfiguration configuration = InitConfiguration();
            IConfiguration storageConfiguration = configuration.GetSection(nameof(AzureStorageSettings));
            AzureStorageSettings azureStorageSettings = new AzureStorageSettings();
            storageConfiguration.Bind(azureStorageSettings);
            ConnectionString = azureStorageSettings.ConnectionString;
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            var table = new AzureCloudTable(ConnectionString, "TestTable");
            await table.CreateIfNotExistsAsync();
            store = new AzureTableMessageStore(table);

        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await store.TryDelete(conversationId, testMessage.UtcTime);
        }

        [TestMethod]
        public async Task AddGetMessage()
        {
            await store.AddMessage(conversationId, testMessage);
            IEnumerable<Message> messageList = await store.ListMessages(conversationId);

            int count = 0;
            foreach(Message message in messageList) {
                count++;
                Assert.AreEqual(testMessage, message);
            }
            
            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        public async Task GetMessagesForNonExistingConversation()
        {
            List<Message> messages = (await store.ListMessages(conversationId)).ToList();
            Assert.AreEqual(messages.Count, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateMessageException))]
        public async Task AddExistingMessage()
        {
            await store.AddMessage(conversationId, testMessage);
            await store.AddMessage(conversationId, testMessage);
        }

        [DataRow("", "Elie", "Fri, 27 Feb 2009 03:11:21 GMT")]
        [DataRow(null, "Elie", "Fri, 27 Feb 2009 03:11:21 GMT")]
        [DataRow("Hello", null, "Fri, 27 Feb 2009 03:11:21 GMT")]
        [DataRow("Hello", "", "Fri, 27 Feb 2009 03:11:21 GMT")]
        [DataRow("Hello", "Elie", null)]
        [TestMethod]
        public async Task AddInvalidMessage(string username, string firstName, string utcTime)
        {
            try
            {
                Message message = new Message(username, firstName, DateTime.Parse(utcTime));
                await store.AddMessage(conversationId, message);

                Assert.Fail($"Expected {nameof(ArgumentException)} was not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]        
        public async Task MessagesShouldComeBackSortedByDateInDecreasingOrder() {
            int N = 50;
            List<DateTime> messageTimes = new List<DateTime>();
            for(int i = 0; i < N; i++) {
                DateTime time = DateTime.FromBinary(100 - i);
                Message message = new Message("Hello", "Elie", time);
                messageTimes.Add(time);
                await store.AddMessage(conversationId, message);
            }

            List<Message> retrievedMessages = (await store.ListMessages(conversationId)).ToList();
            Assert.AreEqual(retrievedMessages.Count, N);
            for(int i = 1; i < retrievedMessages.Count; i++) {
                Assert.IsTrue(DateTime.Compare(retrievedMessages[i].UtcTime, retrievedMessages[i - 1].UtcTime) < 0);
            } 

            await CleanUpAfterSortedMessagesTest(messageTimes);
        }

        private async Task CleanUpAfterSortedMessagesTest(List<DateTime> messageTimes) {
            foreach(DateTime time in messageTimes) {
                await store.TryDelete(conversationId, time);
            }
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json")
            .Build();
            return config;
        }
    }
}