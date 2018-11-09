using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClearChat.Crypto
{
    public class AesStringProtector : IStringProtector
    {
        private readonly byte[] m_Key;

        public AesStringProtector(byte[] key)
        {
            m_Key = key;
        }

        public string Unprotect(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                var provider = GetProvider();
                var iv = new byte[provider.IV.Length];
                memoryStream.Read(iv, 0, provider.IV.Length);

                using (var decryptor = GetProvider().CreateDecryptor(m_Key, iv))
                {
                    using (var csDecrypt = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        public byte[] Protect(string payload)
        {
            var provider = GetProvider();
            provider.GenerateIV();

            using (var srProtect = new MemoryStream())
            {
                srProtect.Write(provider.IV, 0, provider.IV.Length);
                using (var encryptor = provider.CreateEncryptor(m_Key, provider.IV))
                using (var csDecrypt = new CryptoStream(srProtect, encryptor, CryptoStreamMode.Write))
                {
                    var bytes = Encoding.UTF8.GetBytes(payload);
                    csDecrypt.Write(bytes, 0, bytes.Length);
                    csDecrypt.FlushFinalBlock();
                }

                return srProtect.ToArray();
            }
        }

        private AesCryptoServiceProvider GetProvider()
        {
            var c = new AesCryptoServiceProvider
            {
                KeySize = 256,
                Mode = CipherMode.CBC
            };
            return c;
        }
    }
}