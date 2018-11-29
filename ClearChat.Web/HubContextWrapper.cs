using ClearChat.Core;
using Microsoft.AspNetCore.SignalR;

namespace ClearChat.Web
{
    public class HubContextWrapper<T> : IChatHubController where T : Hub
    {
        private readonly IHubContext<T> m_Context;

        public HubContextWrapper(IHubContext<T> context)
        {
            m_Context = context;
        }
    }
}
