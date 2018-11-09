using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using ClearChat.Web.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ClearChat.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var connString = Environment.GetEnvironmentVariable("ClearChat", EnvironmentVariableTarget.Machine);
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

            app.UseSignalR(routes => routes.MapHub<ChatHub>("/chat"));
            app.UseStaticFiles();
        }
    }
}
