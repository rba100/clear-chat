using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
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

        public void Handle(MessageContext context, string arguments)
        {
            var colourStr = arguments.StartsWith("#") ? arguments.Substring(1) : arguments;
            if (arguments.Length != 6) context.MessageHub.PublishSystemMessage(context.ConnectionId, "Must be a six character hex string");

            if (!m_ColourGenerator.ValidColour(colourStr, out string errorMessage))
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, errorMessage);
                return;
            }

            var user = new User(context.User.UserName,
                                colourStr,
                                context.User.VerifiedPublicIdentity);

            m_UserRepository.UpdateUser(user);

            context.MessageHub.PublishUserDetails(new[] { user.UserName });
        }

        public string HelpText => "changes your username colour. Six character RGB hex string.";
    }
}