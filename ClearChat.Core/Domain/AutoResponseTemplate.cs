namespace ClearChat.Core.Domain
{
    public class AutoResponseTemplate
    {
        public int AuthorUserId { get; }
        public int ChannelId { get; }
        public string SubstringTrigger { get; }
        public string Response { get; }

        public AutoResponseTemplate(int authorUserId, int channelId, string substringTrigger, string response)
        {
            AuthorUserId = authorUserId;
            ChannelId = channelId;
            SubstringTrigger = substringTrigger;
            Response = response;
        }
    }
}