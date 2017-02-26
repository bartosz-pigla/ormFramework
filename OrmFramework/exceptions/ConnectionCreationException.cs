using System;

namespace OrmFramework.exceptions
{
    class ConnectionCreationException : Exception
    {
        public ConnectionCreationException(string message)
            : base(message)
        {
        }
    }
}
