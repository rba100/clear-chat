using System.Threading.Tasks;
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

        public Task SignalConnection(string connectionId, string method, object argument)
        {
            return m_Context.Clients.Client(connectionId).SendAsync(method, argument);
        }

        public Task SignalConnection(string connectionId, string method, object arguments1, object arguments2)
        {
            return m_Context.Clients.Client(connectionId).SendAsync(method, arguments1, arguments2);
        }

        public Task SignalChannel(string channelName, string method, params object[] arguments)
        {
            return m_Context.Clients.Group(channelName).SendCoreAsync(method, arguments);
        }

        public void AddToGroup(string connectionId, string channelName)
        {
            m_Context.Groups.AddToGroupAsync(connectionId, channelName);
        }

        public void RemoveFromGroup(string connectionId, string channelName)
        {
            m_Context.Groups.RemoveFromGroupAsync(connectionId, channelName);
        }

        public void SignalAll(string method, object argument)
        {
            m_Context.Clients.All.SendAsync(method, argument);
        }

        public void SignalAll(string method, object argument1, object argument2)
        {
            m_Context.Clients.All.SendAsync(method, argument1, argument2);
        }
    }
}
