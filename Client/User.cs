using OrmFramework.attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Table]
    class User
    {
        [PrimaryKey]
        internal int UserId { get; set; }
        
        [OneToMany]
        internal IEnumerable<Ord> OrderList { get; set; }

        internal string FirstName { get; set; }
    }
}
