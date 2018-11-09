using System;
using ClearChat;
using ClearChat.Crypto;
using ClearChat.Hubs;
using ClearChat.Repositories;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

// ReSharper disable UnusedMember.Global

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
                () => new ChatHub(new SqlServerMessageRepository(connString,
                                                                 new DpApiStringProtector())));
            app.MapSignalR();
        }
    }
}
