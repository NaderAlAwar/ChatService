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
    [TestCategory("Functional")]
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
            var userOne = Guid.NewGuid().ToString();
            var userTwo = Guid.NewGuid().ToString();
            var userThree = Guid.NewGuid().ToString();


            var createProfileDto1 = new CreateProfileDto
            {
                FirstName = userOne, LastName = userOne, Username = userOne
            };

            var createProfileDto2 = new CreateProfileDto
            {
                FirstName = userTwo, LastName = userTwo, Username = userTwo
            };

            var createProfileDto3 = new CreateProfileDto
            {
                FirstName = userThree, LastName = userThree, Username = userThree
            };

            await chatServiceClient.CreateProfile(createProfileDto1);
            await chatServiceClient.CreateProfile(createProfileDto2);
            await chatServiceClient.CreateProfile(createProfileDto3);


            var newConversationDto1 = new CreateConversationDto
            {
                Participants = new[] { userOne, userTwo }
            };

            var newConversationDto2 = new CreateConversationDto
            {
                Participants = new[] { userOne, userThree }
            };

            var conversation1 = await chatServiceClient.AddConversation(newConversationDto1);
            var conversation2 = await chatServiceClient.AddConversation(newConversationDto2);

            var allConversations = await chatServiceClient.ListConversations(userOne);
            string[] recipients =
                {allConversations.Conversations[0].Recipient.Username, allConversations.Conversations[1].Recipient.Username};
            CollectionAssert.AreEquivalent(new[] { userTwo, userThree }, recipients);
            CollectionAssert.AreEquivalent(new[] { conversation1.Id, conversation2.Id }, new[] { allConversations.Conversations[0].Id, allConversations.Conversations[1].Id });
            Assert.AreEqual(userThree, allConversations.Conversations[0].Recipient.Username);
            Assert.AreEqual(userTwo, allConversations.Conversations[1].Recipient.Username);
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
                Assert.AreEqual(HttpStatusCode.NotFound, e.StatusCode);
            }
        }
    }
}