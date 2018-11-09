using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        User GetUser(string id);
        void SaveUser(string id);
    }
}