using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChatService;
using ChatService.Client;
using ChatService.DataContracts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class ConversationsControllerIntegTests
    {
        private HttpClient httpClient;
        private TestServer server;
        private ChatServiceClient chatServiceClient;

        [TestInitialize]
        public void TestInitialize()
        {
            server = new TestServer(WebServer.CreateWebHostBuilder());
            httpClient = server.CreateClient();
            chatServiceClient = new ChatServiceClient(httpClient);
        }

        [TestMethod]
        public async Task CreateConversationTest()
        {
            var newConversationDto1 = new CreateConversationDto
            {
                Participants = new[] { "user1", "user2" }
            };

            var conversation1 = await chatServiceClient.AddConversation(newConversationDto1);
            CollectionAssert.AreEquivalent(new [] { "user1", "user2" }, conversation1.Participants);
        }

        [TestMethod]
        public async Task ListConversationsTest()
        {

            var newConversationDto1 = new CreateConversationDto
            {
                Participants = new[] { "user1", "user2" }
            };

            var newConversationDto2 = new CreateConversationDto
            {
                Participants = new[] { "user1", "user3" }
            };

            var conversation1 = await chatServiceClient.AddConversation(newConversationDto1);
            var conversation2 = await chatServiceClient.AddConversation(newConversationDto2);

            var allConversations = await chatServiceClient.ListConversations("user1");
            string[] recipients =
                {allConversations.Conversations[0].Recipient.Username, allConversations.Conversations[1].Recipient.Username};
            CollectionAssert.AreEquivalent(new[] { "user2", "user3" }, recipients);
            CollectionAssert.AreEquivalent(new[] { conversation1.Id, conversation2.Id }, new[] { allConversations.Conversations[0].Id, allConversations.Conversations[1].Id });
            Assert.AreEqual("user3", allConversations.Conversations[0].Recipient.Username);
            Assert.AreEqual("user2", allConversations.Conversations[1].Recipient.Username);
        }

        [TestMethod]
        public async Task ListConversationsForNonExistingUserTest()
        {
           var allConversations = await chatServiceClient.ListConversations(Guid.NewGuid().ToString());
           Assert.AreEqual(0,allConversations.Conversations.Count);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public async Task ListConversations_InvalidUsername(string username)
        {
            try
            {
                var conversations = await chatServiceClient.ListConversations(username);
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.InternalServerError, e.StatusCode);
            }
        }
    }
}