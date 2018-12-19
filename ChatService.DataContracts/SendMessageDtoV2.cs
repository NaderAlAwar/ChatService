namespace ChatService.DataContracts
{
    public class SendMessageDtoV2 : SendMessageDto
    {
        public SendMessageDtoV2(string text, string senderUsername, string messageId) : base(text, senderUsername)
        {
            MessageId = messageId;
        }

        public string MessageId { get; }
    }
}