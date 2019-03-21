namespace ClearChat.Core.Domain
{
    public class Channel
    {
        public int Id { get; }
        public string Name { get; }
        public bool IsDefault => Name == "default";

        public Channel(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}