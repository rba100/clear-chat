using System.Collections.Generic;
using ClearChat.Core.Domain;
using ClearChat.Core.Exceptions;

namespace ClearChat.Core.Repositories
{
    public interface IAutoResponseRepository
    {
        /// <summary>
        /// Gets auto response for given message
        /// </summary>
        /// <remarks>Can return null</remarks>
        string GetResponse(int channelId, string message);

        /// <summary>
        /// Registers an auto response with the server
        /// </summary>
        /// <exception cref="DuplicateAutoResponseException">
        /// Auto response for given phrase already exists
        /// </exception>
        void AddResponse(int authorUserId, int channelId, string substring, string response);

        void RemoveResponse(int channelId, string substring);

        IReadOnlyCollection<AutoResponseTemplate> GetAll();
    }
}