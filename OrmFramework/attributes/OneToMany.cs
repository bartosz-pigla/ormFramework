using OrmFramework.enumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OneToMany:Attribute
    {
        internal string[] Fields { get; set; }
        internal FetchType Fetch;
        internal Entity Entity;
        
        public OneToMany(FetchType fetch = FetchType.Lazy, params string[] fields)
        {
            Fields = fields;
            Fetch = fetch;
        }
    }
}
