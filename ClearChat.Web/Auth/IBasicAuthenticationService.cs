using System.Threading.Tasks;

namespace ClearChat.Web.Auth
{
    public interface IBasicAuthenticationService
    {
        Task<bool> IsValidUserAsync(string user, string password);
    }
}