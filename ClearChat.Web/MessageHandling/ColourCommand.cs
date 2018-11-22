using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling
{
    public class ColourCommand : ISlashCommand
    {
        private readonly IUserRepository m_UserRepository;
        private readonly IColourGenerator m_ColourGenerator;
        public ColourCommand(IUserRepository userRepository,
                             IColourGenerator colourGenerator)
        {
            m_UserRepository = userRepository;
            m_ColourGenerator = colourGenerator;
        }

        public string CommandText => "colour";

        public void Handle(ChatContext context, string arguments)
        {
            var colourStr = arguments.StartsWith("#") ? arguments.Substring(1) : arguments;
            if(arguments.Length != 6) context.MessageHub.PublishSystemMessage("Must be a six character hex string", MessageScope.Caller);

            if(!m_ColourGenerator.ValidColour(colourStr, out string errorMessage))
            {
                context.MessageHub.PublishSystemMessage(errorMessage, MessageScope.Caller);
                return;
            }

            var newUser = new User(context.User.UserId, colourStr);
            m_UserRepository.UpdateUserDetails(newUser);
        }

        public string HelpText => "changes your user name colour. Six character RGB hex string.";
    }
}