using OrmFramework.attributes;
using OrmFramework.core;
using OrmFramework.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework
{
    class EntityField
    {
        internal PropertyInfo PropertyInfo;
        internal Entity Owner;

        internal PrimaryKey PrimaryKey;
        internal OneToMany OneToMany;
        internal ManyToOne ManyToOne;
        internal Column Column;
        internal NotNull NotNull;

        //internal string OriginalName;
        //internal string Name;

        internal Dictionary<Entity, string> Names;

        internal EntityField(PropertyInfo propertyInfo, Entity entity)
        {
            PropertyInfo = propertyInfo;
            Owner = entity;

            PrimaryKey = (PrimaryKey)Attribute.GetCustomAttribute(propertyInfo, typeof(PrimaryKey));
            OneToMany = (OneToMany)Attribute.GetCustomAttribute(propertyInfo, typeof(OneToMany));
            ManyToOne = (ManyToOne)Attribute.GetCustomAttribute(propertyInfo, typeof(ManyToOne));
            Column = (Column)Attribute.GetCustomAttribute(propertyInfo, typeof(Column));
            NotNull = (NotNull)Attribute.GetCustomAttribute(propertyInfo, typeof(NotNull));

            Names = new Dictionary<Entity, string>();

            Names.Add(entity,GetName());
        }

        //internal EntityField(EntityField field, string name, Entity owner)
        //{
        //    PropertyInfo = field.PropertyInfo;
        //    Owner = owner;
        //    CorrespondingEntity = field.Owner;

        //    PrimaryKey = field.PrimaryKey;
        //    OneToMany = field.OneToMany;
        //    ManyToOne = field.ManyToOne;
        //    Column = field.Column;
        //    NotNull = field.NotNull;

        //    OriginalName = field.Name;
        //    Name = name;

        //    ClassValidator.ValidateField(this);
        //}
        internal void SetCorrespondingEntity()
        {
            if (OneToMany != null)
                SetOneToManyEntity();
            else if (ManyToOne != null)
                SetManyToOneEntity();
        }

        internal void SetOneToManyEntity()
        {
            Type collection = PropertyInfo.PropertyType;
            Type foreignEntityType = collection.GetGenericArguments()[0];
            OneToMany.Entity = OrmManager.GetEntity(foreignEntityType);
        }

        internal void SetManyToOneEntity()
        {
            ManyToOne.Entity = OrmManager.GetEntity(PropertyInfo.PropertyType);
        }

        internal string GetName()
        {
            return Column==null?PropertyInfo.Name:Column.Name;
        }

        internal bool Equals(OneToMany oneToMany)
        {
            if (oneToMany.Fields.Length != ManyToOne.Fields.Length)
                return false;

            for(int i = 0; i < ManyToOne.Fields.Length; i++)
            {
                if (!oneToMany.Fields[i].Equals(ManyToOne.Fields[i]))
                    return false;
            }
            return true;
        }
    }
}
