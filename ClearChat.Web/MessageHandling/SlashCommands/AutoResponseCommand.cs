using System;
using System.Linq;
using System.Text.RegularExpressions;

using ClearChat.Core.Domain;
using ClearChat.Core.Exceptions;
using ClearChat.Core.Repositories;

namespace ClearChat.Web.MessageHandling.SlashCommands
{
    public class AutoResponseCommand : ISlashCommand
    {
        private readonly IAutoResponseRepository m_AutoResponseRepository;
        private readonly IMessageRepository m_MessageRepository;

        public AutoResponseCommand(IAutoResponseRepository autoResponseRepository, 
                                   IMessageRepository messageRepository)
        {
            m_AutoResponseRepository = autoResponseRepository
                ?? throw new ArgumentNullException(nameof(autoResponseRepository));
            m_MessageRepository = messageRepository 
                ?? throw new ArgumentNullException(nameof(messageRepository));
        }

        public string CommandText => "autoresponse";

        public string HelpText => $"Configures auto responses. Use \"/{CommandText} help\" for usage.";

        public void Handle(MessageContext context, string arguments)
        {
            var parameters = SplitParameters(arguments);

            if (parameters.Length == 0 || parameters.First() == "help")
            {
                ShowHelp(context);
                return;
            }

            var command = parameters.First();

            switch (command)
            {
                case "add":
                    if(parameters.Length != 3) ShowHelp(context);
                    else AddResponse(context, parameters[1], parameters[2]);
                    return;
                case "remove":
                    if (parameters.Length != 2) ShowHelp(context);
                    else RemoveResponse(context, parameters[1]);
                    return;
                case "list":
                    //ListResponses(context);
                    return;
            }

            ShowHelp(context);
        }

        private void ShowHelp(MessageContext context)
        {
            context.MessageHub.PublishSystemMessage(context.ConnectionId, "/autoresponse add \"ping\" \"pong\"");
            context.MessageHub.PublishSystemMessage(context.ConnectionId, "/autoresponse remove \"ping\"");
            //context.MessageHub.PublishSystemMessage(context.ConnectionId, "/autoresponse list");
        }

        private void AddResponse(MessageContext context, string substring, string response)
        {
            if (substring.Length < 4)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "Error: that's probably really annoying, so I'm gonna have to pass on that.");
                return;
            }

            try
            {
                var channelInfo = m_MessageRepository.GetChannelInformation(context.ChannelName);
                m_AutoResponseRepository.AddResponse(context.User.Id, channelInfo.Id, substring, response);
            }
            catch (DuplicateAutoResponseException)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Error: Auto response already exists for {substring}");
                return;
            }

            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Successfully registered auto response {response} for message {substring}");
        }

        private void ListResponses(MessageContext context)
        {
            var autoResponses = m_AutoResponseRepository.GetAll();
            foreach (var response in autoResponses)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, $"\"{response.SubstringTrigger}\" => \"{response.Response}\"");
            }

            if (autoResponses.Count == 0)
            {
                context.MessageHub.PublishSystemMessage(context.ConnectionId, "There are none.");
            }
        }

        private void RemoveResponse(MessageContext context, string substring)
        {
            var channel = m_MessageRepository.GetChannelInformation(context.ChannelName);
            m_AutoResponseRepository.RemoveResponse(channel.Id, substring);
            context.MessageHub.PublishSystemMessage(context.ConnectionId, $"Successfully removed auto response for '{substring}'.");
        }

        private static string[] SplitParameters(string input)
        {
            return Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value.Trim('\"'))
                .ToArray();
        }
    }
}