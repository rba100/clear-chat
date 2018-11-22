
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClearChat.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using ClearChat.Web.Auth;
using ClearChat.Web.Hubs;
using ClearChat.Web.MessageHandling;

namespace ClearChat.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connString = Environment.GetEnvironmentVariable("ClearChat", EnvironmentVariableTarget.Machine);
            var hasher = new Sha256StringHasher();
            var msgRepo = new SqlServerMessageRepository(connString,
                                                         new AesStringProtector(new byte[32]),
                                                         hasher);

            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasic<BasicAuthenticationService>(o => o.Realm = "ClearChat");

            services.AddSignalR();
            services.AddTransient<IMessageRepository>(sp => msgRepo);

            var userRepo = new CachingUserRepository(new SqlServerUserRepository(connString,
                                                                                 hasher));
            services.AddSingleton<IUserRepository>(sp => userRepo);
            services.AddSingleton<IConnectionManager>(sp => new ConnectionManager());

            var commands = new ISlashCommand[]
            {
                new ColourCommand(userRepo, new ColourGenerator()),
                new ChangeChannelCommand(msgRepo)
            };

            services.AddSingleton<ISlashCommandHandler>(new SlashCommandHandler(new[]
            {
                new HelpCommand(commands),
            }.Concat(commands)));
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
            app.UseStaticFiles();
            app.UseSignalR(routes => routes.MapHub<ChatHub>("/chatHub"));
        }
    }
}
