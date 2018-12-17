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
    public class ConversationsControllerTests
    {
        readonly Mock<IConversationsStore> conversationsStoreMock = new Mock<IConversationsStore>();
        readonly Mock<IProfileStore> profileStoreMock = new Mock<IProfileStore>();
        readonly Mock<ILogger<ConversationsController>> loggerMock = new Mock<ILogger<ConversationsController>>();
        readonly Mock<IMetricsClient> metricsMock = new Mock<IMetricsClient>();
        readonly Mock<INotificationService> notificationServiceMock = new Mock<INotificationService>();

        [TestInitialize]
        public void TestInitialize()
        {
            metricsMock.Setup(metricsMock => metricsMock.CreateAggregateMetric(It.IsAny<string>()))
                .Returns(new AggregateMetric("TestMetric", metricsMock.Object, TimeSpan.Zero));
        }

        [TestMethod]
        public async Task ListConversationsReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.ListConversations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationsController.ListConversations(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task ListConversationsReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.ListConversations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new UnknownException());

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationsController.ListConversations(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task ListConversationsReturns404WhenProfileNotFoundExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.ListConversations(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new ProfileNotFoundException("test failure"));

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationsController.ListConversations(
                Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 0);

            TestUtils.AssertStatusCode(HttpStatusCode.NotFound, result);
        }

        [TestMethod]
        public async Task CreateConversationReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.AddConversation(It.IsAny<Conversation>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            CreateConversationDto newConversation = new CreateConversationDto()
            {
                Participants = new[] {Guid.NewGuid().ToString(), Guid.NewGuid().ToString()}
            };

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationsController.CreateConversation(
                newConversation);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task CreateConversationReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.AddConversation(It.IsAny<Conversation>()))
                .ThrowsAsync(new UnknownException());

            CreateConversationDto newConversation = new CreateConversationDto()
            {
                Participants = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object, metricsMock.Object, notificationServiceMock.Object);
            IActionResult result = await conversationsController.CreateConversation(
                newConversation);

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        // fake exception used for testing internal server error
        private class UnknownException : Exception
        {
        }

    }
}
