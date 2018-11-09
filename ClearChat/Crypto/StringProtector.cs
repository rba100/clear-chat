using System.Security.Cryptography;
using System.Text;

namespace ClearChat.Crypto
{
    /// <summary>
    /// Provides obfuscation using Windows' built-in data protector API.
    /// </summary>
    /// <remarks>
    /// As the DPAPI is used in 'machine scope', the data can be recovered by any
    /// process running on the machine used to protect the data. If the protected
    /// data is compromised but not the machine that protected it, the data is
    /// secure.
    /// </remarks>
    public class DpApiStringProtector : IStringProtector
    {
        public string Unprotect(byte[] bytes)
        {
            var decoded = ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decoded);
        }

        public byte[] Protect(string payload)
        {
            var bytes = Encoding.UTF8.GetBytes(payload);
            return ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
        }
    }
}