using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class Date :Attribute
    {
        internal string Format { get; set; }
        internal string CultureName { get; set; }

        public Date(string format, string cultureName)
        {
            Format = format;
        }
    }
}
