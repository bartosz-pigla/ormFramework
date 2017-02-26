using System;
using System.Runtime.Serialization;

namespace OrmFramework.exceptions
{
    [Serializable]
    internal class TableAttributeNotFoundException : Exception
    {
        public TableAttributeNotFoundException()
        {
        }

        public TableAttributeNotFoundException(string message) : base(message)
        {
        }

        public TableAttributeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TableAttributeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}