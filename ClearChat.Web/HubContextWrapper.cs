using ClearChat.Core;
using Microsoft.AspNetCore.SignalR;

namespace ClearChat.Web
{
    public class HubContextWrapper<T> : IChatContext where T : Hub
    {
        private readonly IHubContext<T> m_Context;

        public HubContextWrapper(IHubContext<T> context)
        {
            m_Context = context;
        }

        public void SignalConnection(string connectionId, string method, params object[] arguments)
        {
            m_Context.Clients.Client(connectionId).SendCoreAsync(method, arguments);
        }

        public void SignalChannel(string channelName, string method, params object[] arguments)
        {
            m_Context.Clients.Group(channelName).SendCoreAsync(method, arguments);
        }

        public void AddToGroup(string connectionId, string channelName)
        {
            m_Context.Groups.AddToGroupAsync(connectionId, channelName);
        }

        public void RemoveFromGroup(string connectionId, string channelName)
        {
            m_Context.Groups.RemoveFromGroupAsync(connectionId, channelName);
        }
    }
}
