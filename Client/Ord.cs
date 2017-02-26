using OrmFramework.attributes;
using OrmFramework.enums;
using OrmFramework.enumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Table]
    class Ord
    {
        [PrimaryKey]
        internal int OrdId { get; set; }

        [PrimaryKey]
        [ManyToOne(FetchType.Lazy,"ownerId")]
        internal User User { get; set; }

        internal string Name { get; set; }
    }
}
