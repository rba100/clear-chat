﻿
using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using ClearChat.Core;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using ClearChat.Web.Auth;
using ClearChat.Web.Hubs;
using ClearChat.Web.MessageHandling;
using ClearChat.Web.MessageHandling.MessageTransformers;
using ClearChat.Web.MessageHandling.SlashCommands;

namespace ClearChat.Web
{
    public class Startup
    {
        private readonly IConfiguration m_Configuration;

        public Startup(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var environmentVariableConnString = Environment.GetEnvironmentVariable("ClearChat",
                                                                                   EnvironmentVariableTarget.Machine);

            var connString = string.IsNullOrWhiteSpace(environmentVariableConnString) 
                ? m_Configuration.GetConnectionString("clear-chat") 
                : environmentVariableConnString;

            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new ApplicationException("No connection string has been configured. See Startup.cs");
            }

            var hasher = new Sha256StringHasher();
            var msgRepo = new ChannelCachingMessageRepository(new SqlServerMessageRepository(
                 connString,
                 new AesStringProtector(new byte[32]),
                 hasher));

            services.AddSignalR();
            services.AddSingleton<IChatContext>(sp => new HubContextWrapper<ChatHub>(sp.GetService<IHubContext<ChatHub>>()));
            services.AddSingleton<IMessageRepository>(sp => msgRepo);
            services.AddSingleton<IAutoResponseRepository>(sp => new RateLimitingAutoResponseRepository(
                                                                     new CachingAutoResponseRepository(
                                                                         new AutoResponseRepository(connString, hasher),
                                                                         hasher), TimeSpan.FromMinutes(20)));
            services.AddSingleton<IStringHasher>(sp => hasher);
            services.AddSingleton<IColourGenerator, ColourGenerator>();
            services.AddSingleton<IUserRepository>(sp => new CachingUserRepository(
                new SqlServerUserRepository(connString, hasher, sp.GetService<IColourGenerator>())));
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IMessageHandler>(s => new CompositeMessageHandler(new IMessageHandler[]
            {
                new SlashCommandMessageHandler(new ISlashCommand[]
                {
                    new ColourCommand(s.GetService<IUserRepository>(),s.GetService<IColourGenerator>()),
                    new JoinChannelCommand(s.GetService<IMessageRepository>(), s.GetService<IConnectionManager>()),
                    new InviteSlashCommand(s.GetService<IMessageRepository>(), s.GetService<IUserRepository>(), s.GetService<IConnectionManager>()),
                    new PurgeChannelCommand(s.GetService<IMessageRepository>(), s.GetService<IConnectionManager>(), s.GetService<IUserRepository>()),
                    new LeaveChannelCommand(s.GetService<IMessageRepository>(), s.GetService<IConnectionManager>()),
                    new DeleteMessageCommand(s.GetService<IMessageRepository>()),
                    new AutoResponseCommand(s.GetService<IAutoResponseRepository>()),
                    new UploadSlashCommand(s.GetService<IMessageRepository>())
                }),
                new ChannelPermissionHandler(),
                new ChatMessageHandler(msgRepo,
                                       s.GetService<IAutoResponseRepository>(),
                                       s.GetService<IUserRepository>(), 
                                       new []{ new ImageLinkMessageTransformer() })
            }));

            services.AddSingleton<IMessageHub, ChatController>();
            
            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasic<BasicAuthenticationService>(o => o.Realm = "ClearChat");

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseChallengeOnPath("/", returnTo: "/");
            app.UseChallengeOnPathAlways("/changeIdentity", returnTo: "/");
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });
            app.UseSignalR(routes => routes.MapHub<ChatHub>("/chatHub"));
            app.UseMvc();
        }
    }
}
