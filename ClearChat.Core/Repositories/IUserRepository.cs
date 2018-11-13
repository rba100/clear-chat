using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        bool UserIdExists(string userId);
        void SaveUser(string userId, string password);
        bool ValidateUser(string userId, string password);
    }
}