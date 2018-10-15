using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChatService;
using ChatService.Client;
using ChatService.DataContracts;
using ChatService.Storage;
using ChatServiceTests.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ChatService.Providers;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Functional")]
    public class MessageControllerIntegrationTests
    {
        private HttpClient httpClient;
        private TestServer server;
        private ChatServiceClient chatServiceClient;
        private string conversationId;

        [TestInitialize]
        public void TestInitialize()
        {
            server = new TestServer(WebServer.CreateWebHostBuilder());
            httpClient = server.CreateClient();
            chatServiceClient = new ChatServiceClient(httpClient);
        }

        [TestMethod]
        public async Task CreateGetMessage()
        {
            string userOne = Guid.NewGuid().ToString();
            string userTwo = Guid.NewGuid().ToString();
            var conversation = await chatServiceClient.AddConversation(new CreateConversationDto()
                { Participants = new[] {userOne, userTwo } });
            var sendMessageDto = new SendMessageDto(
                "Hello",
                userOne
            );

            await chatServiceClient.SendMessage(conversation.Id, sendMessageDto);
            List<ListMessagesItemDto> messages = (await chatServiceClient.ListMessages(conversation.Id)).Messages;

            Assert.AreEqual(messages.Count, 1);
            Assert.AreEqual(messages[0].SenderUsername, sendMessageDto.SenderUsername);
            Assert.AreEqual(messages[0].Text, sendMessageDto.Text);
        }

        [TestMethod]
        public async Task AttemptToGetMessagesFromNonExistingConversation()
        {
            List<ListMessagesItemDto> messages = (await chatServiceClient.ListMessages(Guid.NewGuid().ToString())).Messages;
            Assert.AreEqual(messages.Count, 0);
        }

        [TestMethod]
        public async Task SendDuplicateMessageShouldReturn409()
        {
            DateTime currentTime = DateTime.UtcNow;
            Mock<ITimeProvider> timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock.Setup(timeProvider => timeProvider.GetCurrentTimeUtc())
                .Returns(currentTime);
            InjectCustomTimeProviderToMessageController(timeProviderMock.Object);

            string userOne = Guid.NewGuid().ToString();
            string userTwo = Guid.NewGuid().ToString();
            var conversation = await chatServiceClient.AddConversation(new CreateConversationDto()
                { Participants = new[] { userOne, userTwo } });
            var sendMessageDto = new SendMessageDto(
                "Hello",
                userOne
            );

            await chatServiceClient.SendMessage(conversation.Id, sendMessageDto);

            try
            {
                await chatServiceClient.SendMessage(conversation.Id, sendMessageDto);
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.Conflict, e.StatusCode);
            }
        }

        [TestMethod]
        public async Task ListMessagesShouldComeBackSortedInReverseChronologicalOrder() {
            IncreasingTimeProvider increasingTimeProvider = new IncreasingTimeProvider();
            InjectCustomTimeProviderToMessageController(increasingTimeProvider);
            string userOne = Guid.NewGuid().ToString();
            string userTwo = Guid.NewGuid().ToString();
            var conversation = await chatServiceClient.AddConversation(new CreateConversationDto()
                { Participants = new[] { userOne, userTwo } });
            var sendMessageDto = new SendMessageDto(
                "Hello",
                userOne
            );

            for(int i = 0; i < 10; i++) {
                await chatServiceClient.SendMessage(conversation.Id, sendMessageDto);
            }

            List<ListMessagesItemDto> messages = (await chatServiceClient.ListMessages(conversation.Id)).Messages;

            Assert.AreEqual(messages.Count, 10);
            for(int i = 1; i < 10; i++) {
                Assert.IsTrue(DateTime.Compare(messages[i].UtcTime, messages[i - 1].UtcTime) < 0);
            }
        }

        [DataRow("", "Elie")]
        [DataRow(null, "Elie")]
        [DataRow("Hello", null)]
        [DataRow("Hello", "")]
        [TestMethod]
        public async Task SendMessage_InvalidDto(string text, string senderUsername)
        {
            var sendMessageDto = new SendMessageDto(
                text,
                senderUsername
            );

            try
            {
                await chatServiceClient.SendMessage(Guid.NewGuid().ToString(), sendMessageDto);
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
            }
        }

        private void InjectCustomTimeProviderToMessageController(ITimeProvider timeProvider)
        {
            server = new TestServer(new WebHostBuilder()
            .ConfigureServices(
                services => services.AddSingleton<ITimeProvider>(timeProvider)
            ).UseStartup<Startup>());

            httpClient = server.CreateClient();
            chatServiceClient = new ChatServiceClient(httpClient);

        }
    }
}
