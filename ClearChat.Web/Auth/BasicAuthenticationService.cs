using System.Threading.Tasks;

namespace ClearChat.Web.Auth
{
    class BasicAuthenticationService : IBasicAuthenticationService
    {
        public Task<bool> IsValidUserAsync(string user, string password)
        {
            return Task.FromResult(true);
        }
    }
}