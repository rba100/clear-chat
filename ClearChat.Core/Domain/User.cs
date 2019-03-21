namespace ClearChat.Core.Domain
{
    public class User
    {
        public int Id { get; }
        public string UserName { get; }
        public string HexColour { get; }

        /// <summary>
        /// User is identified as a named person to all users of the system and that identity is confirmed.
        /// </summary>
        /// <remarks>
        /// This grants access to features that are easy to abuse.
        /// </remarks>
        public bool VerifiedPublicIdentity { get; }


        public User(int id,
                    string userName, 
                    string hexColour, 
                    bool verifiedPublicIdentity)
        {
            Id = id;
            UserName = userName;
            HexColour = hexColour;
            VerifiedPublicIdentity = verifiedPublicIdentity;
        }
    }
}