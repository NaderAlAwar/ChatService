using System;
using System.Threading.Tasks;
using ChatService.DataContracts;
using ChatService.Notifications;
using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class ServiceBusNotificationServiceClientTests
    {
        readonly Mock<IQueueClient> queueClientMock = new Mock<IQueueClient>();
        private ServiceBusNotificationServiceClient notificationServiceClient;

        [TestInitialize]
        public void TestInitialize()
        {
            notificationServiceClient = new ServiceBusNotificationServiceClient(queueClientMock.Object);
        }

        [TestMethod]
        [DataRow("", "foo", new[]{"foo", "bar"})]
        [DataRow(null, "foo", new[] { "foo", "bar" })]
        [DataRow("foo", null, new[] { "foo", "bar" })]
        [DataRow("foo", "", new[] { "foo", "bar" })]
        [DataRow("foo", "foo", null)]
        [DataRow("foo", "foo", new string[]{})]
        [DataRow("foo", "foo", new[]{ "", "bar"})]
        [DataRow("foo", "foo", new[] { null, "bar" })]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendNotificationPayloadInvalidStringsTest(string type, string conversationId, string[] users)
        {
            DateTime utcTime = DateTime.Now;
            var payload = new NotificationPayload(utcTime, type, conversationId, users);
            await notificationServiceClient.SendNotificationAsync(payload);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendNotificationNullPayloadTest()
        {
            await notificationServiceClient.SendNotificationAsync(null);
        }
        
    }
}