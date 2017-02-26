using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class Column:Attribute
    {
        internal string Name { get; set; }
        internal int Length { get; set; }

        public Column(string name, int length=255)
        {
            Name = name;
            Length = length;
        }
    }
}
