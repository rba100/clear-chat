namespace ClearChat.Core.Domain
{
    public class User
    {
        public string UserId { get; }
        public string HexColour { get; }

        /// <summary>
        /// User is identified as a named person to all users of the system and that identity is confirmed.
        /// </summary>
        /// <remarks>
        /// This grants access to features that are easy to abuse.
        /// </remarks>
        public bool VerifiedPublicIdentity { get; }

        public User(string userId, string hexColour, bool verifiedPublicIdentity)
        {
            UserId = userId;
            HexColour = hexColour;
            VerifiedPublicIdentity = verifiedPublicIdentity;
        }
    }
}