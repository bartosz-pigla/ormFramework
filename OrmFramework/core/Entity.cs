using OrmFramework.attributes;
using OrmFramework.connections;
using OrmFramework.core;
using OrmFramework.enums;
using OrmFramework.enumTypes;
using OrmFramework.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework
{
    class Entity
    {
        internal List<EntityField> Primary;
        internal List<EntityField> ManyToOne;
        internal List<EntityField> OneToMany;
        internal List<EntityField> Foreign;
        internal List<EntityField> Standard;
        internal List<EntityField> All;

        internal Resource Resource;

        internal Entity InheritedEntity;

        internal Table Table;
        internal Type Class;

        internal Connection Connection;

        internal Entity(Type type)
        {
            Resource = new Resource(this);

            Primary = new List<EntityField>();
            ManyToOne = new List<EntityField>();
            OneToMany = new List<EntityField>();
            Foreign = new List<EntityField>();
            Standard = new List<EntityField>();
            All = new List<EntityField>();

            Class = type;

            Table = (Table)Attribute.GetCustomAttribute(type, typeof(Table));
            if (Table.Name == string.Empty)
                Table.Name = type.Name;

            AddFields();
        }
       

        private void AddFields()
        {
            InheritedEntity = OrmManager.GetBaseEntity(Class);
            if (Table.TableType == TableType.InheritedFieldsToChild)
            {
                OrmManager.Entities.Remove(InheritedEntity);
                AddFields(BindingFlags.NonPublic | BindingFlags.Instance);
            }
            else
            {
                AddFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            }
        }

        //private void AddAllParentFields()
        //{
        //    Primary.AddRange(InheritedEntity.Primary);
        //    ManyToOne.AddRange(InheritedEntity.ManyToOne);
        //    OneToMany.AddRange(InheritedEntity.OneToMany);
        //    Standard.AddRange(InheritedEntity.Standard);
        //    All.AddRange(InheritedEntity.All);
        //}
        private void AddFields(BindingFlags flags)
        {
            foreach (PropertyInfo property in Class.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                All.Add(new EntityField(property, this));
            }

            foreach (EntityField field in All)
            {
                AddField(field);
            }
        }

        private void AddField(EntityField field)
        {
            if (field.PrimaryKey != null)
                Primary.Add(field);

            if (field.ManyToOne != null)
                ManyToOne.Add(field);

            if (field.OneToMany != null)
                OneToMany.Add(field);

            if (field.PrimaryKey == null &&
                field.ManyToOne == null &&
                field.OneToMany == null)
                Standard.Add(field);
        }

        //private void AddParentAsForeignField()
        //{
        //    foreach (EntityField field in InheritedEntity.Primary)
        //    {
        //        All.Add(field);
        //        AddField(field);
        //    }
        //}

        internal static void SetRelation(Entity entity)
        {
            if (entity.Primary.Count == 0)
                throw new EntityException(EntityError.LackOfPrimaryKey);

            foreach (EntityField field in entity.All)
            {
                if (field.PrimaryKey != null && field.OneToMany != null)
                    throw new EntityException(EntityError.PrimaryCannotBeOneToMany);
                if (field.OneToMany != null && field.ManyToOne != null)
                    throw new EntityException(EntityError.OneToManyCannotBeManyToOne);

                if (field.OneToMany != null)
                {
                    try
                    {
                        field.SetOneToManyEntity();
                    }
                    catch (Exception exc)
                    {
                        throw new EntityException(EntityError.InvalidOneToManyForeignEntity);
                    }

                    if (field.OneToMany.Entity == null)
                        throw new EntityException(EntityError.InvalidOneToManyForeignEntity);
                }
                else if (field.ManyToOne != null)
                {
                    try
                    {
                        field.SetManyToOneEntity();
                    }
                    catch (Exception exc)
                    {
                        throw new EntityException(EntityError.IvalidManyToOneForeignEntity);
                    }

                    if (field.ManyToOne.Entity == null)
                        throw new EntityException(EntityError.IvalidManyToOneForeignEntity);
                }
            }
        }


    }
}
