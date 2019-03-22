namespace ClearChat.Core.Domain
{
    public class User
    {
        /// <summary>
        /// Persistence ID. Use the UserName field instead for references outside of the core library.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// The chosen name of the user.
        /// </summary>
        /// <remarks>
        /// Case must be preserved by all consumers.
        /// When dealing with user-submitted information, silently handle:
        ///  - Comparisons must be case insensitive.
        ///  - All leading and trailing white-space must be trimmed.
        ///  - All internal white-space must be collapsed to a single space character (char 32).
        /// but throw an exception if:
        ///  - Funny characters in string.
        ///  - String really long.
        ///  - String is a reserved word.
        ///
        /// Where these rules are ambiguous, YOU decide what happens. Yes, YOU, in front of this monitor!
        /// </remarks>
        public string UserName { get; }

        /// <summary>
        /// The user's favourite colour, for their username colouration in UIs.
        /// </summary>
        /// <remarks>
        /// Must be a six character hexadecimal representation of a 24-bit RGB colour.
        /// Consumers should check user-submitted values to ensure they are visible
        /// against a white background.
        /// </remarks>
        public string HexColour { get; }

        /// <summary>
        /// User is identified as a named person to all users of the system and that identity is confirmed.
        /// </summary>
        /// <remarks>
        /// This grants access to features that are easy to abuse.
        /// </remarks>
        public bool VerifiedPublicIdentity { get; }

        public User(string userName,
                    string hexColour,
                    bool verifiedPublicIdentity)
        {
            Id = 0;
            UserName = userName;
            HexColour = hexColour;
            VerifiedPublicIdentity = verifiedPublicIdentity;
        }

        /// <summary>
        /// Internal constructor that sets an ID field.
        /// </summary>
        internal User(int id,
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