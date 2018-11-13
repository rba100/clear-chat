namespace ClearChat.Core.Domain
{
    public class User
    {
        public string UserId { get; }
        public string HexColour { get; }

        public User(string userId, string hexColour)
        {
            UserId = userId;
            HexColour = hexColour;
        }
    }
}