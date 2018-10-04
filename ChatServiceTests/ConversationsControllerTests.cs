using System;
using System.Net;
using System.Threading.Tasks;
using ChatService.Controllers;
using ChatService.DataContracts;
using ChatService.Storage;
using ChatServiceTests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class ConversationsControllerTests
    {
        readonly Mock<IProfileStore> profileStoreMock = new Mock<IProfileStore>();
        readonly Mock<IConversationsStore> conversationsStoreMock = new Mock<IConversationsStore>();
        readonly Mock<ILogger<ConversationsController>> loggerMock = new Mock<ILogger<ConversationsController>>();


        private CreateConversationDto createConversationDto = new CreateConversationDto
        {
            Participants = new[] {"foo", "bar"}
        };

        [TestMethod]
        public async Task ListConversationsReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.ListConversations(It.IsAny<string>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object);
            IActionResult result = await conversationsController.ListConversations("foo");

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task ListConversationsReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.ListConversations(It.IsAny<string>()))
                .ThrowsAsync(new UnknownException());

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object);
            IActionResult result = await conversationsController.ListConversations("foo");

            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        [TestMethod]
        public async Task CreateConversationReturns503WhenStorageIsUnavailable()
        {
            conversationsStoreMock.Setup(store => store.AddConversation(It.IsAny<Conversation>()))
                .ThrowsAsync(new StorageErrorException("Test Failure"));

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object);
            IActionResult result = await conversationsController.CreateConversation(createConversationDto);

            TestUtils.AssertStatusCode(HttpStatusCode.ServiceUnavailable, result);
        }

        [TestMethod]
        public async Task CreateConversationReturns500WhenUnknownExceptionIsThrown()
        {
            conversationsStoreMock.Setup(store => store.AddConversation(It.IsAny<Conversation>()))
                .ThrowsAsync(new UnknownException());

            var conversationsController = new ConversationsController(conversationsStoreMock.Object, profileStoreMock.Object, loggerMock.Object);
            IActionResult result = await conversationsController.CreateConversation(createConversationDto);
            TestUtils.AssertStatusCode(HttpStatusCode.InternalServerError, result);
        }

        private class UnknownException : Exception
        {
        }
    }


}