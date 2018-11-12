
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using ClearChat.Web.Auth;
using ClearChat.Web.Hubs;
using Microsoft.AspNetCore.Authentication;

namespace ClearChat.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connString = Environment.GetEnvironmentVariable("ClearChat", EnvironmentVariableTarget.Machine);

            services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
                    .AddBasic<BasicAuthenticationService>(o => o.Realm = "ClearChat");

            services.AddSignalR();
            services.AddTransient<IMessageRepository>(sp => new SqlServerMessageRepository(connString,
                                                                                           new AesStringProtector(new byte[32])));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.Use((ctx, next) =>
            {
                var isChangeIdentity = ctx.Request.Path == "/changeIdentity";
                if (!isChangeIdentity) return next();

                var done = ctx.Request.Cookies.TryGetValue("IdentityStage", out string val) && val == "done";
                if (!done)
                {
                    ctx.Response.Cookies.Append("IdentityStage", "done");
                    return ctx.ChallengeAsync();
                }

                ctx.Response.Cookies.Append("IdentityStage", "notdone");
                ctx.Response.Redirect("/");
                return Task.CompletedTask;
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSignalR(routes => routes.MapHub<ChatHub>("/chatHub"));
        }
    }
}
