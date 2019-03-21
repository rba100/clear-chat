
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ClearChat.Core.Repositories;
using Newtonsoft.Json;

namespace ClearChat.Web.Api
{
    [Route("api/attachment")]
    [ApiController]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly IMessageRepository m_MessageRepository;

        public AttachmentController(IMessageRepository messageRepository)
        {
            m_MessageRepository = messageRepository;
        }

        // TODO Channel Security
        [HttpGet]
        [Route("{attachmentId}")]
        public void Get(int attachmentId)
        {
            var attachment = m_MessageRepository.GetAttachment(attachmentId);
            if (attachment == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Response.ContentType = attachment.ContentType;
            Response.Body.Write(attachment.Content);
            Response.Body.Flush();
        }

        [HttpGet]
        [Route("{attachmentId}/type")]
        public void GetType(int attachmentId)
        {
            var attachment = m_MessageRepository.GetAttachment(attachmentId);
            if (attachment == null)
            {
                Response.StatusCode = 404;
                return;
            }

            Response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(attachment.ContentType));
            Response.Body.Write(bytes);
            Response.Body.Flush();
        }
    }
}
