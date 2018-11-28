using System;
using System.Linq;
using System.Threading.Tasks;
using ChatService.Client;
using ChatService.DataContracts;
using ChatService.Storage;
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
                Participants = new[] { participant1, participant2 }
            };

            await Task.WhenAll(
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant1, FirstName = "Participant", LastName = "1" }),
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
        public async Task ConversationsPaging()
        {
            string participant1 = RandomString();
            string participant2 = RandomString();
            string participant3 = RandomString();
            string participant4 = RandomString();

            await Task.WhenAll(
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant1, FirstName = "Participant", LastName = "1" }),
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant2, FirstName = "Participant", LastName = "2" }),
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant3, FirstName = "Participant", LastName = "3" }),
                chatServiceClient.CreateProfile(new CreateProfileDto { Username = participant4, FirstName = "Participant", LastName = "4" })
            );

            await chatServiceClient.AddConversation(new CreateConversationDto { Participants = new[] { participant1, participant2 } });
            await chatServiceClient.AddConversation(new CreateConversationDto { Participants = new[] { participant1, participant3 } });
            await chatServiceClient.AddConversation(new CreateConversationDto { Participants = new[] { participant1, participant4 } });

            ListConversationsDto participant1ConversationsDto = await chatServiceClient.ListConversations(participant1, limit: 2);
            Assert.AreEqual(2, participant1ConversationsDto.Conversations.Count);
            Assert.AreEqual(participant4, participant1ConversationsDto.Conversations[0].Recipient.Username);
            Assert.AreEqual(participant3, participant1ConversationsDto.Conversations[1].Recipient.Username);

            ListConversationsDto participant2ConversationsDto = await chatServiceClient.ListConversations(participant2);
            Assert.AreEqual(1, participant2ConversationsDto.Conversations.Count);
            Assert.AreEqual(participant1, participant2ConversationsDto.Conversations[0].Recipient.Username);

            // fetch previous page
            participant1ConversationsDto = await chatServiceClient.ListConversationsByUri(participant1ConversationsDto.PreviousUri);
            Assert.AreEqual(1, participant1ConversationsDto.Conversations.Count);
            Assert.AreEqual(participant2, participant1ConversationsDto.Conversations[0].Recipient.Username);

            // fetch previous page again but there is nothing
            var dto = await chatServiceClient.ListConversationsByUri(participant1ConversationsDto.PreviousUri);
            Assert.AreEqual(0, dto.Conversations.Count);
            Assert.IsTrue(string.IsNullOrWhiteSpace(dto.PreviousUri));
            Assert.IsTrue(string.IsNullOrWhiteSpace(dto.NextUri));

            // fetch next page
            participant1ConversationsDto = await chatServiceClient.ListConversationsByUri(participant1ConversationsDto.NextUri);
            Assert.AreEqual(2, participant1ConversationsDto.Conversations.Count);
            Assert.AreEqual(participant4, participant1ConversationsDto.Conversations[0].Recipient.Username);
            Assert.AreEqual(participant3, participant1ConversationsDto.Conversations[1].Recipient.Username);

            // fetch next page
            dto = await chatServiceClient.ListConversationsByUri(participant1ConversationsDto.NextUri);
            Assert.AreEqual(0, dto.Conversations.Count);
            Assert.IsTrue(string.IsNullOrWhiteSpace(dto.PreviousUri));
            Assert.IsTrue(string.IsNullOrWhiteSpace(dto.NextUri));
        }

        [TestMethod]
        public async Task MessagesPaging()
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
            DateTime dateTime = DateTime.UtcNow;

            var messages = new[] {
                new SendMessageDto("Hola what's up?", participant1),
                new SendMessageDto("Not much you?", participant2),
                new SendMessageDto("Writing some code!", participant1),
                new SendMessageDto("Cool! Are you taking EECE503E?", participant2)
            };

            foreach(SendMessageDto message in messages) {
                await chatServiceClient.SendMessage(conversationDto.Id, message);
            }

            ListMessagesDto messagesPayload = await chatServiceClient.ListMessages(conversationDto.Id, 3);
            Assert.AreEqual(3, messagesPayload.Messages.Count);
            Assert.AreEqual(messages[3].Text, messagesPayload.Messages[0].Text);
            Assert.AreEqual(messages[2].Text, messagesPayload.Messages[1].Text);
            Assert.AreEqual(messages[1].Text, messagesPayload.Messages[2].Text);

            messagesPayload = await chatServiceClient.ListMessagesByUri(messagesPayload.PreviousUri);
            Assert.AreEqual(1, messagesPayload.Messages.Count);
            Assert.AreEqual(messages[0].Text, messagesPayload.Messages[0].Text);

            messagesPayload = await chatServiceClient.ListMessagesByUri(messagesPayload.PreviousUri);
            Assert.AreEqual(0, messagesPayload.Messages.Count);
            Assert.AreEqual(messagesPayload.PreviousUri, "");
            Assert.AreEqual(messagesPayload.PreviousUri, "");
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
