using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using ClearChat.Web.Auth;
using ClearChat.Web.Hubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

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
                var isLogout = ctx.Request.Path == "/logout";

                if (isLogout)
                {
                    ctx.Response.Cookies.Append("Logout", "true");
                    ctx.Response.Redirect("/");
                    return Task.CompletedTask;
                }

                var logout = ctx.Request.Cookies.TryGetValue("Logout", out string val) && val == "true";

                if (!ctx.User.Identity.IsAuthenticated || logout)
                {
                    ctx.Response.Cookies.Append("Logout", "false");
                    ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    ctx.Response.Headers["WWW-Authenticate"] = $"Basic realm=\"ClearChat\", charset=\"UTF-8\"";
                    return Task.CompletedTask;
                }
                else
                {
                    return next();
                }
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSignalR(routes => routes.MapHub<ChatHub>("/chatHub"));
        }
    }
}
