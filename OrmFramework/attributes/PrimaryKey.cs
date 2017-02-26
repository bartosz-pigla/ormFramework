using System;

namespace OrmFramework.attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false,Inherited = true)]
    public class PrimaryKey:Attribute
    {
        public PrimaryKey()
        {

        }
    }
}
