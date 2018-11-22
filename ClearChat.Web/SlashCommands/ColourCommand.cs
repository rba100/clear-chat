using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.SlashCommands
{
    public class ColourCommand : ISlashCommand
    {
        private readonly IUserRepository m_UserRepository;

        public ColourCommand(IUserRepository userRepository)
        {
            m_UserRepository = userRepository;
        }

        public string CommandText => "colour";

        public void Handle(ChatContext context, string arguments)
        {
            var colourStr = arguments.StartsWith("#") ? arguments.Substring(1) : arguments;
            if(arguments.Length != 6) context.MessageHub.PublishSystemMessage("Must be a six character hex string", MessageScope.Caller);
            var newUser = new User(context.User.UserId, colourStr);
            m_UserRepository.UpdateUserDetails(newUser);
        }

        public string HelpText => "changes your user name colour. Six character RGB hex string.";
    }
}