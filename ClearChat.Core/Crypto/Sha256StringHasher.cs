using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ClearChat.Core.Crypto
{
    public class Sha256StringHasher : IStringHasher
    {
        public byte[] Hash(string payload, byte[] salt)
        {
            var hasher = new SHA256CryptoServiceProvider();
            return hasher.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(payload))
                                          .ToArray());
        }

        public bool HashMatch(string match, byte[] hash, byte[] salt)
        {
            var hasher = new SHA256CryptoServiceProvider();
            return hasher.ComputeHash(salt.Concat(Encoding.UTF8.GetBytes(match))
                                          .ToArray())
                         .SequenceEqual(hash);
        }
    }
}