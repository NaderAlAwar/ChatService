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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Metrics;
using ChatServiceTests.Utils;
using Polly;

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
        readonly Mock<ILogger<ConversationController>> loggerMock = new Mock<ILogger<ConversationController>>();
        readonly Mock<IMetricsClient> metricsMock = new Mock<IMetricsClient>();
        readonly Mock<INotificationService> notificationServiceMock = new Mock<INotificationService>();

        [TestInitialize]
        public void TestInitialize()
        {
            metricsMock.Setup(metricsMock => metricsMock.CreateAggregateMetric(It.IsAny<string>()))
                .Returns(new AggregateMetric("TestMetric", metricsMock.Object, TimeSpan.Zero));
        }

        [TestMethod]
        public async Task ListMessagesReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.ListMessages(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var conversationController = new ConversationController(conversationsStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationController.ListMessages(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task ListMessagesReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.ListMessages(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new UnknownException());

            var conversationController = new ConversationController(conversationsStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationController.ListMessages(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task PostMessageReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.AddMessage(It.IsAny<string>(), It.IsAny<Message>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            SendMessageDto newMessage = new SendMessageDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var conversationController = new ConversationController(conversationsStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationController.PostMessage(
                Guid.NewGuid().ToString(), newMessage);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task PostMessageReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.AddMessage(It.IsAny<string>(), It.IsAny<Message>()))
                .ThrowsAsync(new UnknownException());

            SendMessageDto newMessage = new SendMessageDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            var conversationController = new ConversationController(conversationsStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
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
