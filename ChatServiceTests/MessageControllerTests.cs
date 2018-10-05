using System;
using System.Net;
using ChatService.Controllers;
using ChatService.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatServiceTests.Utils;
using Microsoft.AspNetCore.Mvc;
using ChatService.Providers;

namespace ChatServiceTests
{
    /// <summary>
    /// In this class we mostly test the edge cases that we cannot test in our integration tests.
    /// It's fine to have overlap between these tests and the controller integrations tests.
    /// </summary>
    [TestClass]
    [TestCategory("UnitTests")]
    public class MessageControllerTests
    {
        readonly Mock<IMessageStore> messageStoreMock = new Mock<IMessageStore>();
        readonly Mock<ILogger<MessageController>> loggerMock = new Mock<ILogger<MessageController>>();
        readonly Mock<ITimeProvider> timeProviderMock = new Mock<ITimeProvider>();

        private SendMessageDto sendMessageDto = new SendMessageDto("Hello", "Elie");

        [TestMethod]
        public async Task AddMessageReturns503WhenStorageIsUnavailable()
        {
            messageStoreMock.Setup(store => store.AddMessage(It.IsAny<string>(), It.IsAny<Message>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var messageController = new MessageController(messageStoreMock.Object, loggerMock.Object, timeProviderMock.Object);
            IActionResult result = await messageController.PostMessage(
                "I am a conversationId", sendMessageDto);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task AddMessageReturns500WhenUnknownExceptionIsThrown()
        {
            messageStoreMock.Setup(store => store.AddMessage(It.IsAny<string>(), It.IsAny<Message>()))
                .ThrowsAsync(new UnknownException());

            var messageController = new MessageController(messageStoreMock.Object, loggerMock.Object, timeProviderMock.Object);
            IActionResult result = await messageController.PostMessage(
                "I am a conversationId", sendMessageDto);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task ListMessagesReturns503WhenStorageIsUnavailable()
        {
            const string conversationId = "I am a conversationId";
            messageStoreMock.Setup(store => store.ListMessages(conversationId))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var messageController = new MessageController(messageStoreMock.Object, loggerMock.Object, timeProviderMock.Object);
            IActionResult result = await messageController.ListMessages(conversationId);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task GetProfileReturns500WhenUnknownExceptionIsThrown()
        {
            const string conversationId = "I am a conversationId";
            messageStoreMock.Setup(store => store.ListMessages(conversationId))
                .ThrowsAsync(new UnknownException());

            var messageController = new MessageController(messageStoreMock.Object, loggerMock.Object, timeProviderMock.Object);
            IActionResult result = await messageController.ListMessages(conversationId);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        // fake exception used for testing internal server error
        private class UnknownException : Exception
        {
        }

    }
}
