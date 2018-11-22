using System.Collections.Generic;
using System.Linq;

namespace ChatService.Storage
{
    public class SortedMessagesWindow
    {
        public string StartCt { get; }
        public string EndCt { get; }
        public IEnumerable<Message> Messages { get; }

        public SortedMessagesWindow(IEnumerable<Message> messages, string startCt, string endCt)
        {
            Messages = messages;
            StartCt = startCt;
            EndCt = endCt;
        }
    }
}
