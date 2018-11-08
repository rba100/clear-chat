using ClearChat.Models;

namespace ClearChat.Repositories
{
    public interface IUserRepository
    {
        User GetUser(string id);
        void SaveUser(string id);
    }
}