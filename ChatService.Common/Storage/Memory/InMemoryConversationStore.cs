using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatService.Storage.Memory
{
    public class InMemoryConversationStore : IConversationsStore
    {
        private readonly Dictionary<string, SortedUserConversationList> userConversations = new Dictionary<string, SortedUserConversationList>();
        private readonly Dictionary<string, List<Message>> conversationsMessages = new Dictionary<string, List<Message>>();

        public Task AddConversation(Conversation conversation)
        {
            if (conversationsMessages.ContainsKey(conversation.Id))
            {
                return Task.CompletedTask;
            }
            conversationsMessages.Add(conversation.Id, new List<Message>());

            foreach(string participantUsername in conversation.Participants)
            {
                AddConversation(participantUsername, conversation);
            }
            return Task.CompletedTask;
        }

        public Task AddMessage(string conversationId, Message message)
        {
            if (!conversationsMessages.TryGetValue(conversationId, out var messageList))
            {
                throw new ConversationNotFoundException($"The conversation {conversationId} does not exists");
            }

            messageList.Add(message);

            Conversation conversation = userConversations[message.SenderUsername].GetConversation(conversationId);
            MarkModified(conversation);

            return Task.CompletedTask;
        }

        private void MarkModified(Conversation conversation)
        {
            foreach(string user in conversation.Participants)
            {
                userConversations[user].MarkModified(conversation.Id);
            }
        }

        public Task<SortedConversationsWindow> ListConversations(string username, string startCt, string endCt, int limit)
        {
            if (!userConversations.TryGetValue(username, out var conversationList))
            {
                return Task.FromResult(new SortedConversationsWindow(Enumerable.Empty<Conversation>(), "", ""));
            }

            return Task.FromResult(new SortedConversationsWindow(conversationList.SortedConversations, "", ""));
        }

        public Task<SortedMessagesWindow> ListMessages(string conversationId, string startCt, string endCt, int limit)
        {
            if (conversationsMessages.TryGetValue(conversationId, out var messageList))
            {
                return Task.FromResult(new SortedMessagesWindow(messageList.AsEnumerable(), "", ""));
            }
            return Task.FromResult(new SortedMessagesWindow(Enumerable.Empty<Message>(), "", ""));
        }

        private Task AddConversation(string username, Conversation conversation)
        {
            if (userConversations.TryGetValue(username, out SortedUserConversationList conversationList))
            {
                conversationList.AddConversation(conversation);
            }
            else
            {
                conversationList = new SortedUserConversationList();
                conversationList.AddConversation(conversation);
                userConversations.Add(username, conversationList);
            }
            return Task.CompletedTask;
        }

        public Task<Conversation> GetConversation(string username, string conversationId)
        {
            throw new System.NotImplementedException();
        }
    }
}
