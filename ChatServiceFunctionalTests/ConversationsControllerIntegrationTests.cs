using System;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Client;
using ChatService.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceFunctionalTests
{
    [TestClass]
    [TestCategory("Integration")]
    [TestCategory("Functional")]
    public class ConversationsControllerIntegrationTests
    {
        private ChatServiceClient chatServiceClient;

        [TestInitialize]
        public void TestInitialize()
        {
            chatServiceClient = TestUtils.CreateTestServerAndClient();
        }

        [TestMethod]
        public async Task CreateListConversations()
        {
            string participant1 = RandomString();
            string participant2 = RandomString();
            var createConversationDto = new CreateConversationDto
            {
                Participants = new[] {participant1, participant2}
            };

            await Task.WhenAll(
                chatServiceClient.CreateProfile(new CreateProfileDto {Username = participant1, FirstName = "Participant", LastName = "1"}),
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant2, FirstName = "Participant", LastName = "2" })
            );

            await chatServiceClient.AddConversation(createConversationDto);

            ListConversationsDto participant1ConversationsDto = await chatServiceClient.ListConversations(participant1);
            Assert.AreEqual(1, participant1ConversationsDto.Conversations.Count);

            ListConversationsDto participant2ConversationsDto = await chatServiceClient.ListConversations(participant2);
            Assert.AreEqual(1, participant2ConversationsDto.Conversations.Count);

            ListConversationsItemDto participant1ConversationItemDto = participant1ConversationsDto.Conversations.First();
            ListConversationsItemDto participant2ConversationItemDto = participant2ConversationsDto.Conversations.First();
            Assert.AreEqual(participant1ConversationItemDto.Id, participant2ConversationItemDto.Id);
            Assert.AreEqual(participant1ConversationItemDto.LastModifiedDateUtc, participant2ConversationItemDto.LastModifiedDateUtc);

            Assert.AreEqual(participant1, participant2ConversationItemDto.Recipient.Username);
            Assert.AreEqual(participant2, participant1ConversationItemDto.Recipient.Username);
        }

        [TestMethod]
        public async Task AddListMessages()
        {
            string participant1 = RandomString();
            string participant2 = RandomString();
            var createConversationDto = new CreateConversationDto
            {
                Participants = new[] { participant1, participant2 }
            };

            await Task.WhenAll(
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant1, FirstName = "Participant", LastName = "1" }),
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant2, FirstName = "Participant", LastName = "2" })
            );

            var conversationDto = await chatServiceClient.AddConversation(createConversationDto);
            var message1 = new SendMessageDto("Hello", participant1);
            var message2 = new SendMessageDto("What's up?", participant1);
            var message3 = new SendMessageDto("Not much!", participant2);
            await chatServiceClient.SendMessage(conversationDto.Id, message1);
            await chatServiceClient.SendMessage(conversationDto.Id, message2);
            await chatServiceClient.SendMessage(conversationDto.Id, message3);

            ListMessagesDto listMessagesDto = await chatServiceClient.ListMessages(conversationDto.Id);
            Assert.AreEqual(3, listMessagesDto.Messages.Count);
            Assert.AreEqual(message3.Text, listMessagesDto.Messages[0].Text);
            Assert.AreEqual(message2.Text, listMessagesDto.Messages[1].Text);
            Assert.AreEqual(message1.Text, listMessagesDto.Messages[2].Text);
        }

        private static string RandomString()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
