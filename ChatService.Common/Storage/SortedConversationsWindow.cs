using System.Collections.Generic;
using System.Linq;

namespace ChatService.Storage
{
    public class SortedConversationsWindow
    {
        public string StartCt { get; }
        public string EndCt { get; }
        public IEnumerable<Conversation> Conversations { get; }

        public SortedConversationsWindow(IEnumerable<Conversation> conversations, string startCt, string endCt)
        {
            Conversations = conversations;
            StartCt = startCt;
            EndCt = endCt;
        }
    }
}