﻿using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public interface IUserRepository
    {
        bool UserIdExists(string userId);
        User GetUserDetails(string userId);
        void SaveUser(string userId, string password);
        void UpdateUser(User user);
        bool ValidateUser(string userId, string password);
    }
}