using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableUserStoreIntegrationTests
    {
        private static string ConnectionString { get; set; }
        private AzureTableUserStore usersStore;
        private DateTime validDateTime = DateTime.Now;
        private readonly Conversation testConversation = new Conversation(Guid.NewGuid().ToString(), new[]{"foo","bar"}, DateTime.Now);

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
            var usersTable = new AzureCloudTable(ConnectionString, "usersTestTable");

            await usersTable.CreateIfNotExistsAsync();
            
            usersStore = new AzureTableUserStore(usersTable);
        }

        [TestMethod]
        [DataRow("", new string[]{"foo","bar"},"ignore")]
        [DataRow(null, new string[] { "foo", "bar" }, "ignore")]
        [DataRow("foo_bar", new string[] { }, "ignore")]
        [DataRow("foo_bar", null, "ignore")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddInvalidConversation(string id, string[] participants, string ignore)
        {
            Conversation test = new Conversation(id,participants,validDateTime);
            await usersStore.AddConversation(test);
        }

        [TestMethod]
        [DataRow("", new string[] { "foo", "bar" }, "ignore")]
        [DataRow(null, new string[] { "foo", "bar" }, "ignore")]
        [DataRow("foo_bar", new string[] { }, "ignore")]
        [DataRow("foo_bar", null, "ignore")]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateInvalidConversation(string id, string[] participants, string ignore)
        {
            Conversation test = new Conversation(id, participants, validDateTime);
            await usersStore.UpdateConversation(test);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(null)]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ListInvalidUsernameConversations(string username)
        {
            await usersStore.ListConversations(username);
        }

        [TestMethod]
        [ExpectedException(typeof(ConversationNotFoundException))]
        public async Task UpdateNonExistingConversation()
        {
            await usersStore.UpdateConversation(testConversation);
        }

        [TestMethod]
        public async Task ListConversationsNonExistingUser()
        {
            var conversations = await usersStore.ListConversations(Guid.NewGuid().ToString());
            Assert.AreEqual(0, conversations.Count());
        }

        [TestMethod]
        public async Task AddListConversations()
        {
            var firstId = Guid.NewGuid().ToString();
            var secondId = Guid.NewGuid().ToString();

            var conversation1 = new Conversation(firstId,new string[]{"one","two"}, DateTime.Now);
            var conversation2 = new Conversation(secondId, new string[] { "one","three" }, DateTime.Now.AddSeconds(1));

            await usersStore.AddConversation(conversation1);
            await usersStore.AddConversation(conversation2);

            var conversations = await usersStore.ListConversations("one");
            var enumerable = conversations.ToList();
            Assert.AreEqual(2, enumerable.Count());
            Assert.AreEqual(secondId, enumerable[0].Id);
            Assert.AreEqual(firstId, enumerable[1].Id);

        }

        [TestMethod]
        public async Task AddUpdateConversations()
        {
            var firstId = Guid.NewGuid().ToString();
            var time1 = DateTime.Now;
            var time2 = time1.AddSeconds(1);

            var conversation1 = new Conversation(firstId, new string[] { "one","two" }, time1);
            await usersStore.AddConversation(conversation1);
            var conversation2 = new Conversation(firstId, new string[] { "one","two" }, time2);
            await usersStore.UpdateConversation(conversation2);

            var retrievedConversation = await usersStore.retrieveEntity("one", firstId);
            var retrievedDateTime = retrievedConversation.TicksToDateTime();
            Assert.AreEqual(time2.ToString(), retrievedDateTime.ToString());
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