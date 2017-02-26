using System;
using System.Runtime.Serialization;

namespace OrmFramework.connections
{
    [Serializable]
    internal class PrimaryKeyException : Exception
    {
        public PrimaryKeyException()
        {
        }

        public PrimaryKeyException(string message) : base(message)
        {
        }

        public PrimaryKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PrimaryKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}