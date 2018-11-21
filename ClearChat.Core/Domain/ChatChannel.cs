namespace ClearChat.Core.Domain
{
    public class ChatChannel
    {
        public string Name { get; }

        public ChatChannel(string name)
        {
            Name = name;
        }
    }
}