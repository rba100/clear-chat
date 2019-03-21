using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        bool UserNameExists(string userName);
        User GetUserDetails(string userName);
        User GetUserDetails(int userId);
        void SaveUser(User user, string password);
        void UpdateUser(User user);
        bool ValidateUser(string userName, string password);
    }
}