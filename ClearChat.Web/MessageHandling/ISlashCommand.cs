using ClearChat.Core.Domain;

namespace ClearChat.Web.MessageHandling
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
        void Handle(MessageContext context, string arguments);

        /// <summary>
        /// Short description of what the command does.
        /// </summary>
        string HelpText { get; }
    }
}
