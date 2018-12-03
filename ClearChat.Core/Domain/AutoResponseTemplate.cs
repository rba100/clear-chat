namespace ClearChat.Core.Domain
{
    public class AutoResponseTemplate
    {
        public byte[] CreatorIdHash { get; }
        public string SubstringTrigger { get; }
        public string Response { get; }

        public AutoResponseTemplate(byte[] creatorIdHash, string substringTrigger, string response)
        {
            CreatorIdHash = creatorIdHash;
            SubstringTrigger = substringTrigger;
            Response = response;
        }
    }
}