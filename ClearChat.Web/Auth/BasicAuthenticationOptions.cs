using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace ClearChat.Web.Auth
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string Realm { get; set; }
    }
}
