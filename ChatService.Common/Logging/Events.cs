using Microsoft.Extensions.Logging;

namespace ChatService.Logging
{
    public static class Events
    {
        public static readonly EventId ProfileCreated = CreateEvent(nameof(ProfileCreated));
        public static readonly EventId InternalError = CreateEvent(nameof(InternalError));
        public static readonly EventId ProfileNotFound = CreateEvent(nameof(ProfileNotFound));
        public static readonly EventId ProfileAlreadyExists = CreateEvent(nameof(ProfileAlreadyExists));
        public static readonly EventId StorageError = CreateEvent(nameof(StorageError));
        public static readonly EventId ConversationMessageAdded = CreateEvent(nameof(ConversationMessageAdded));
        public static readonly EventId ConversationCreated = CreateEvent(nameof(ConversationCreated));

        private static EventId CreateEvent(string eventName)
        {
            return new EventId(0, eventName);
        }
    }
}
