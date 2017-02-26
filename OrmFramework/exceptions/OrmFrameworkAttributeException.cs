using System;

namespace OrmFramework.exceptions
{
    class OrmFrameworkAttributeException : Exception
    {
        public OrmFrameworkAttributeException(string message)
            : base(message)
        {
        }
    }
}
