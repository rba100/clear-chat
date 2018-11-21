using System;
using ClearChat.Core.Repositories;

namespace ClearChat.Core.Domain
{
    public class ChatMessage
    {
        public ChatMessage(string userId,
                           string channelName, 
                           string message,
                           string colour,
                           DateTime timeStampUtc)
        {
            UserId = userId;
            ChannelName = channelName;
            Message = message;
            TimeStampUtc = timeStampUtc;
            Colour = colour;
        }

        public string UserId { get; }
        public string ChannelName { get; }
        public string Message { get; }
        public DateTime TimeStampUtc { get; }
        public string Colour { get; }
    }

    public class MessageFactory
    {
        private readonly IColourGenerator m_ColourGenerator;
        private readonly IUserRepository m_UserRepository;

        public MessageFactory(IUserRepository userRepository)
        {
            m_ColourGenerator = new ColourGenerator();
            m_UserRepository = userRepository;
        }

        public ChatMessage Create(string userId, string message, string channelName, DateTime timeStampUtc)
        {
            var colour = m_UserRepository.GetUserDetails(userId)?.HexColour ??
                         m_ColourGenerator.GenerateFromString(userId);

            return new ChatMessage(
                userId,
                channelName, 
                message, 
                colour,
                timeStampUtc);
        }
    }
}