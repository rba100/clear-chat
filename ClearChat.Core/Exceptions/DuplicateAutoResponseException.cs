using System;
using System.Runtime.Serialization;

namespace ClearChat.Core.Exceptions
{
    [Serializable]
    public sealed class DuplicateAutoResponseException : Exception
    {
        public DuplicateAutoResponseException()
            : base("Duplicate auto response found")
        {}

        public DuplicateAutoResponseException(string message) : base(message)
        {}

        public DuplicateAutoResponseException(string message, Exception innerException)
            : base(message, innerException)
        {}

        private DuplicateAutoResponseException(SerializationInfo info,
                                               StreamingContext context)
            : base(info, context)
        {}
    }
}
