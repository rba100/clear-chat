using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ClearChat.Web.Auth
{
    public static class ChallengeExtensionMEthods
    {
        public static void UseChallengeOnPath(this IApplicationBuilder app, string challengePath, string returnTo)
        {
            app.Use((ctx, next) => InvokeAsync(ctx, next, challengePath, returnTo));
        }

        private static Task InvokeAsync(HttpContext ctx, Func<Task> next, string challengePath, string returnUrl)
        {
            var isChangeIdentity = ctx.Request.Path == challengePath;
            if (!isChangeIdentity) return next();

            var done = ctx.Request.Cookies.TryGetValue("IdentityStage", out string val) && val == "done";
            if (!done)
            {
                ctx.Response.Cookies.Append("IdentityStage", "done");
                return ctx.ChallengeAsync();
            }

            ctx.Response.Cookies.Append("IdentityStage", "not-done");
            ctx.Response.Redirect(returnUrl);
            return Task.CompletedTask;
        }
    }
}