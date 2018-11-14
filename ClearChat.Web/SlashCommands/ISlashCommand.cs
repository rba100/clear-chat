using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;
using ClearChat.Web.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace ClearChat.Web.SlashCommands
{
    public interface ISlashCommand
    {
        /// <summary>
        /// The text which invokes the command.
        /// </summary>
        /// <remarks>
        /// E.g.m if CommandText is "help" then "/help" invokes this command.
        /// </remarks>
        string CommandText { get; }

        /// <summary>
        /// A function which handles the invocation of the command.
        /// </summary>
        void Handle(User user, IMessageSink messageSink, string arguments);

        /// <summary>
        /// Short description of what the command does.
        /// </summary>
        string HelpText { get; }
    }

    public class ColourCommand : ISlashCommand
    {
        private readonly IUserRepository m_UserRepository;

        public ColourCommand(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public string CommandText => "colour";

        public void Handle(User user, IMessageSink messageSink, string arguments)
        {
            var colourStr = arguments.StartsWith("#") ? arguments.Substring(1) : arguments;
            if(arguments.Length != 6) messageSink.PublishSystemMessage("Must be a six character hex string", "default", MessageScope.Caller);
            var newUser = new User(user.UserId, colourStr);
            m_UserRepository.UpdateUserDetails(newUser);
        }

        public string HelpText => "changes your user name colour. Six character RGB hex string.";
    }
}
