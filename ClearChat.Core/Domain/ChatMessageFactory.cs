using System;
using ClearChat.Core.Repositories;

namespace ClearChat.Core.Domain
{
    /// <summary>
    /// Generates instances of ChatMessage. Chooses the colour if the User doesn't have one set.
    /// </summary>
    public class ChatMessageFactory
    {
        private readonly IColourGenerator m_ColourGenerator;
        private readonly IUserRepository m_UserRepository;

        public ChatMessageFactory(IUserRepository userRepository)
        {
            m_ColourGenerator = new ColourGenerator();
            m_UserRepository = userRepository;
        }

        public ChatMessage Create(string userId, string message, string channelName, DateTime timeStampUtc)
        {
            var colour = m_UserRepository.GetUserDetails(userId)?.HexColour ??
                         m_ColourGenerator.GenerateFromString(userId);

            return new ChatMessage(userId,
                                   channelName, 
                                   message, 
                                   colour,
                                   timeStampUtc);
        }
    }
}