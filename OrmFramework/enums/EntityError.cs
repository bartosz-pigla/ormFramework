using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.enums
{
    public enum EntityError
    {
        LackOfPrimaryKey,
        InvalidOneToManyForeignEntity,
        ForeignAttributeMismatch,
        PrimaryCannotBeOneToMany,
        OneToManyMustBeCollection,
        ForeignCannotBeCollection,
        OneToManyCannotBeManyToOne,
        PrimaryForeignKeyIsNotTable,
        IvalidManyToOneForeignEntity
    }
}
