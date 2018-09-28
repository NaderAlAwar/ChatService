using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatService.Storage.Memory
{
    using ConversationEntry = Tuple<string, Conversation>;
    using LinkedListNode = LinkedListNode<Tuple<string, Conversation>>;

    public class SortedUserConversationList
    {
        private readonly Dictionary<string, LinkedListNode> conversations = new Dictionary<string, LinkedListNode>();
        private readonly LinkedList<ConversationEntry> orderList = new LinkedList<ConversationEntry>();

        public IEnumerable<Conversation> SortedConversations => orderList.Select(entry => entry.Item2);

        public void AddConversation(Conversation conversation)
        {
            ConversationEntry entry = Tuple.Create(conversation.Id, conversation);
            LinkedListNode node = orderList.AddFirst(entry);
            conversations.Add(conversation.Id, node);
        }

        public void RemoveConversation(string conversationId)
        {
            if (conversations.TryGetValue(conversationId, out var node))
            {
                orderList.Remove(node);
                conversations.Remove(conversationId);
            }
        }

        public Conversation GetConversation(string conversationId)
        {
            return conversations[conversationId].Value.Item2;
        }

        public void MarkModified(string conversationId)
        {
            if (conversations.TryGetValue(conversationId, out var node))
            {
                if (orderList.First != node)
                {
                    var conversation = node.Value.Item2;
                    conversation = new Conversation(conversation.Id, conversation.Participants, DateTime.UtcNow);

                    RemoveConversation(conversationId);
                    AddConversation(conversation);
                }
            }
        }
    }
}
