using System;
using ClearChat;
using ClearChat.Hubs;
using ClearChat.Repositories;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace ClearChat
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var connString = Environment.GetEnvironmentVariable("ClearChat", EnvironmentVariableTarget.Machine);
            GlobalHost.DependencyResolver.Register(
                typeof(ChatHub),
                () => new ChatHub(new SqlServerMessageRepository(connString)));
            app.MapSignalR();
        }
    }
}
