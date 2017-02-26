using OrmFramework.enums;
using OrmFramework.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.core
{
    class Resource
    {
        private List<EntityField> _primary;
        private List<EntityField> _foreign;

        internal List<EntityField> Primary {
            get
            {
                if (_primary == null)
                {
                    _primary = GetPrimaryFields();
                }
                return _primary;
            }
            private set
            {
                _primary = value;
            }
        }

        internal List<EntityField> Foreign
        {
            get
            {
                if (_foreign == null)
                {
                    _foreign = GetForeignFields();
                }
                return _foreign;
            }
            private set
            {
                _foreign = value;
            }
        }

        internal List<EntityField> Standard;
        internal List<EntityField> All;

        internal Dictionary<Entity, List<EntityField>> ForeignEntityPrimaryFields;

        internal Entity Entity;

        internal Resource(Entity entity)
        {
            Entity = entity;
        }

        void SetAllFields()
        {
            All = new List<EntityField>();
            foreach(EntityField field in Primary.Concat(Foreign).Concat(Standard))
            {
                if (!All.Contains(field))
                    All.Add(field);
            }
        }

        internal void Convert()
        {
            //Primary = GetPrimaryFields(entity);
            //Foreign = GetForeignFields(entity);
            Standard = Entity.Standard;
            SetAllFields();

            ForeignEntityPrimaryFields = new Dictionary<Entity, List<EntityField>>();

            Dictionary<EntityField, Entity> temp = new Dictionary<EntityField, Entity>();
            
            foreach(EntityField field in Foreign)
            {
                if(!temp.Keys.Contains(field))
                    temp.Add(field, GetForeignEntity(field));
            }

            foreach(EntityField field in Foreign)
            {
                Entity foreignEntity = temp[field];
                if (!ForeignEntityPrimaryFields.Keys.Contains(foreignEntity))
                {
                    ForeignEntityPrimaryFields[foreignEntity] = new List<EntityField>();
                }
                ForeignEntityPrimaryFields[foreignEntity].Add(field);
            }

            //foreach(Entity foreignEntity in foreignEntitySet)
            //{
            //    List<EntityField> foreignFields = Foreign.Where(foreign => foreign.CorrespondingEntity == foreignEntity).ToList();
            //    //Entity.SetManyToOnePropertiesNames(foreignFields,)
            //    Dictionary.Add(foreignEntity, Foreign.Where(foreign => foreign.CorrespondingEntity == foreignEntity));
            //}


        }

        internal Entity GetForeignEntity(EntityField foreignField)
        {
            foreach(EntityField field in Entity.ManyToOne)
            {
                Entity foreignEntity = field.ManyToOne.Entity;
                List<EntityField> primaryFieldsOfForeignEntity = foreignEntity.Resource.Primary;
                if (primaryFieldsOfForeignEntity.Contains(foreignField))
                    return foreignEntity;
            }
            return null;
        }

        internal List<EntityField> GetForeignFields()
        {
            List<EntityField> list = new List<EntityField>();
            foreach (EntityField field in Entity.ManyToOne)
            {
                List<EntityField> primaryFields = field.ManyToOne.Entity.Resource.Primary;
                //List<EntityField> primaryFields = GetPrimaryFields(OrmManager.GetEntity(field.PropertyInfo.PropertyType));
                //list.AddRange(primaryFields);
                list.AddRange(SetManyToOnePropertiesNames(primaryFields, field));
            }
            return list;

            //List<EntityField> list = new List<EntityField>();
            //foreach(EntityField field in entity.ManyToOne.Except(entity.Primary))
            //{
            //    List<EntityField> primaryFields = GetPrimaryFields(OrmManager.GetEntity(field.PropertyInfo.PropertyType));
            //    list.AddRange(SetManyToOnePropertiesNames(primaryFields, field));
            //}
            //return list;
        }
        internal List<EntityField> GetPrimaryFields()
        {
            List<EntityField> list1 = new List<EntityField>();
            foreach (EntityField field in Entity.Primary)
            {
                if (field.ManyToOne == null)
                {
                    list1.Add(field);
                }
                else
                {
                    List<EntityField> manyToOneFields = field.ManyToOne.Entity.Resource.Primary;
                    //List<EntityField> manyToOneFields = GetPrimaryFields(OrmManager.GetEntity(field.PropertyInfo.PropertyType));
                    //List<EntityField> manyToOneFields = GetForeignFields(OrmManager.GetEntity(field.PropertyInfo.PropertyType));
                    //list1.AddRange(manyToOneFields);

                    list1.AddRange(SetManyToOnePropertiesNames(manyToOneFields, field));
                }
            }
            return list1;
        }
        internal static List<EntityField> SetManyToOnePropertiesNames(List<EntityField> manyToOneFields, EntityField field)
        {
            List<EntityField> list = new List<EntityField>();

            if (manyToOneFields.Count != field.ManyToOne.Fields.Length)
                throw new EntityException(EntityError.ForeignAttributeMismatch);

            for (int i = 0; i < manyToOneFields.Count; i++)
            {
                EntityField foreignField = manyToOneFields[i];

                if (!foreignField.Names.Keys.Contains(field.Owner))
                    foreignField.Names.Add(field.Owner, field.ManyToOne.Fields[i]);

                //EntityField newForeignField = new EntityField(foreignField, field.ManyToOne.Fields[i], field.Owner);
                list.Add(foreignField);
            }
            return list;
        }
    }
}
