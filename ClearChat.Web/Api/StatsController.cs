using System.Linq;
using ClearChat.Core;
using ClearChat.Core.Crypto;
using ClearChat.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearChat.Web.Api
{
    [Route("api/stats")]
    [ApiController]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly IConnectionManager m_ConnectionManager;
        private readonly IMessageRepository m_MessageRepository;
        private readonly IStringHasher m_StringHasher;

        public StatsController(IConnectionManager connectionManager, 
                               IMessageRepository messageRepository, 
                               IStringHasher stringHasher)
        {
            m_ConnectionManager = connectionManager;
            m_MessageRepository = messageRepository;
            m_StringHasher = stringHasher;
        }

        [HttpGet, Route("users")]
        public object Users()
        {
            var users = m_ConnectionManager.GetUsers();
            return users.ToDictionary(u => u, m_MessageRepository.GetChannelMembershipsForUser);
        }

        [HttpGet, Route("channel/{channelName}")]
        public object Channel(string channelName)
        {
            var users = m_ConnectionManager.GetUsers();
            if (channelName == "default") return users;
            var memberships = m_MessageRepository.GetChannelMembershipsForChannel(channelName);
            return users.Where(u =>
            {
                var hash = m_StringHasher.Hash(u);
                return memberships.Any(m => m.SequenceEqual(hash));
            }).ToArray();
        }
    }
}