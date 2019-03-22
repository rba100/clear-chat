using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        bool UserNameExists(string userName);
        User GetUser(string userName);
        User GetUser(int userId);
        User SaveUser(User user, string password);
        void UpdateUser(User user);
        bool ValidateUser(string userName, string password);
    }
}