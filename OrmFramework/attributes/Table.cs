using OrmFramework.enumTypes;
using System;

namespace OrmFramework.attributes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =true)]
    public class Table:Attribute
    {
        internal string Name { get; set; }
        internal TableType TableType { get; set; }
        
        public Table(TableType tableType = TableType.InheritedFieldsToChild)
        {
            Name = string.Empty;
            TableType = tableType;
        }

        public Table(string name, TableType tableType= TableType.InheritedFieldsToChild)
        {
            Name = name;
            TableType = tableType;
        }
    }
}
