using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ClearChat.Core.Crypto;
using ClearChat.Core.Domain;

namespace ClearChat.Core.Repositories
{
    public class CachingAutoResponseRepository : IAutoResponseRepository
    {
        private readonly IAutoResponseRepository m_InnerRepository;
        private readonly IStringHasher m_Hasher;

        private readonly ConcurrentDictionary<string, AutoResponseTemplate> m_Cache;

        public CachingAutoResponseRepository(IAutoResponseRepository innerRepository, IStringHasher hasher)
        {
            m_InnerRepository = innerRepository;
            m_Hasher = hasher;
            m_Cache = new ConcurrentDictionary<string, AutoResponseTemplate>(m_InnerRepository.GetAll()
                .ToDictionary(i => i.SubstringTrigger, i => i));
        }

        public string GetResponse(string message)
        {
            return m_Cache.FirstOrDefault(kvp => message.Contains(kvp.Key)).Value?.Response;
        }

        public void AddResponse(string creatorId, string substring, string response)
        {
            m_InnerRepository.AddResponse(creatorId, substring, response);
            m_Cache.TryAdd(substring, new AutoResponseTemplate(m_Hasher.Hash(creatorId),
                substring, response));
        }

        public void RemoveResponse(string substring)
        {
            m_InnerRepository.RemoveResponse(substring);
            m_Cache.TryRemove(substring, out _);
        }

        public IReadOnlyCollection<AutoResponseTemplate> GetAll()
        {
            return m_Cache.Values.ToArray();
        }
    }
}