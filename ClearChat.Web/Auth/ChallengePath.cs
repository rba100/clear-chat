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
            app.Use((ctx, next) => InvokeAsync(ctx, next, challengePath, returnTo, false));
        }

        public static void UseChallengeOnPathAlways(this IApplicationBuilder app, string challengePath, string returnTo)
        {
            app.Use((ctx, next) => InvokeAsync(ctx, next, challengePath, returnTo, true));
        }

        private static Task InvokeAsync(HttpContext ctx, Func<Task> next, string challengePath, string returnUrl, bool forceAuthenticate)
        {
            var isChallengePath = ctx.Request.Path == challengePath;
            if (!isChallengePath) return next();
            var challenging = ctx.Request.Cookies.TryGetValue("IdentityStage", out string val) &&
                              val == "challenging";

            var shouldLogin = forceAuthenticate || !ctx.User.Identity.IsAuthenticated;
            var waitChallengeResponse = ctx.User.Identity.IsAuthenticated && challenging;

            if (shouldLogin && !waitChallengeResponse)
            {
                ctx.Response.Cookies.Append("IdentityStage", "challenging");
                return ctx.ChallengeAsync();
            }

            if (waitChallengeResponse)
            {
                ctx.Response.Cookies.Append("IdentityStage", "ready");
                ctx.Response.Redirect(returnUrl);
                return Task.CompletedTask;
            }

            return next();
        }
    }
}