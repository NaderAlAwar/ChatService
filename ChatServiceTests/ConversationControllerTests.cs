using System;
using System.Net;
using ChatService.Controllers;
using ChatService.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Notifications;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Metrics;
using ChatServiceTests.Utils;

namespace ChatServiceTests
{
    /// <summary>
    /// In this class we mostly test the edge cases that we cannot test in our integration tests.
    /// It's fine to have overlap between these tests and the controller integrations tests.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public class ConversationControllerTests
    {
        readonly Mock<IConversationsStore> conversationsStoreMock = new Mock<IConversationsStore>();
        readonly Mock<ILogger<ConversationService>> serviceLoggerMock = new Mock<ILogger<ConversationService>>();
        readonly Mock<ILogger<ConversationController>> controllerLoggerMock = new Mock<ILogger<ConversationController>>();
        readonly Mock<INotificationService> notificationServiceMock = new Mock<INotificationService>();
        private ConversationService conversationService;
        private ConversationController conversationController;

        [TestInitialize]
        public void TestInitialize()
        {
            conversationService = new ConversationService(conversationsStoreMock.Object, serviceLoggerMock.Object,
                notificationServiceMock.Object);
            conversationController = new ConversationController(controllerLoggerMock.Object, conversationService);
        }

        [TestMethod]
        public async Task ListMessagesReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.ListMessages(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            IActionResult result = await conversationController.ListMessages(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task ListMessagesReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.ListMessages(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new UnknownException());

            IActionResult result = await conversationController.ListMessages(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task PostMessageReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.TryGetMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            SendMessageDto newMessage = new SendMessageDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            IActionResult result = await conversationController.PostMessage(
                Guid.NewGuid().ToString(), newMessage);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task PostMessageReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.TryGetMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnknownException());

            SendMessageDto newMessage = new SendMessageDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            IActionResult result = await conversationController.PostMessage(
                Guid.NewGuid().ToString(), newMessage);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task PostMessageV2Returns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.TryGetMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            SendMessageDtoV2 newMessage = new SendMessageDtoV2(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            IActionResult result = await conversationController.PostMessage(
                Guid.NewGuid().ToString(), newMessage);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task PostMessageV2Returns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.TryGetMessage(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnknownException());

            SendMessageDtoV2 newMessage = new SendMessageDtoV2(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            IActionResult result = await conversationController.PostMessage(
                Guid.NewGuid().ToString(), newMessage);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        // fake exception used for testing internal server error
        private class UnknownException : Exception
        {
        }

    }
}
