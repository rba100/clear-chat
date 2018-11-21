using System;
using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Domain;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.SlashCommands
{
    class ChangeChannelSlashCommand : ISlashCommand
    {
        private readonly IMessageRepository m_MessageRepository;

        public ChangeChannelSlashCommand(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        public string CommandText => "channel";

        public void Handle(User user, IMessageSink messageSink, string arguments)
        {
            var parts = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!parts.Any())
            {
                messageSink.PublishSystemMessage("Error: correct usage is /channel channelName channelPassword", 
                                                 MessageScope.Caller);
                return;
            }

            var channelName = parts[0];
            var password = parts.Length > 1 ? parts[1] : string.Empty;
            if (!m_MessageRepository.GetOrCreateChannel(channelName, password))
            {
                messageSink.PublishSystemMessage("Error: wrong password channel",
                                                 MessageScope.Caller);
                return;
            }
            messageSink.ChangeChannel(channelName);
        }

        public string HelpText => "Switch channel with: /channel channelName [password]";
    }
}