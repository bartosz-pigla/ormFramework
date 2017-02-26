using OrmFramework.enums;
using OrmFramework.enumTypes;
using OrmFramework.exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OrmFramework.core;

namespace OrmFramework.connections
{
    internal abstract class Connection
    {
        internal DatabaseOperation CurrentOperation;

        internal abstract void Create(Entity entity);

        internal abstract void DropTable(Entity entity);

        internal abstract IEnumerable Read(int pageNumber, int rowNumber, Entity entity);

        internal abstract IEnumerable<T> GetIterator<T>(int bufferSize, Predicate<T> predicate);

        internal abstract object Read(Entity entity, params object[] ids);

        internal abstract object Fetch(Entity entity, params object[] ids);

        internal abstract IEnumerable LazyRead(Entity entity, List<EntityField> fields, object[] values);

        internal abstract IEnumerable<object> ReadForeign(Entity entity, EntityField parentEntityField, object parentObject, List<EntityField> foreignFields, object[] foreignIds);

        internal abstract void Delete(Entity entity, List<EntityField> fields, object[] values);

        internal abstract void InsertUpdate(Entity entity, DatabaseOperation operation, params object[] objects);

        internal abstract void InsertUpdate(Entity entity, object obj, Dictionary<EntityField, object> dictionary);

        internal abstract IEnumerable<T> Query<T>(string query);

        internal abstract void Connect();

        internal abstract void Disconnect();

        internal static void Delete(Entity entity, object obj)
        {
            Dictionary<EntityField, object> fieldValues = Connection.GetIds(entity, obj);
            List<EntityField> fields = fieldValues.Keys.ToList();
            object[] values = fieldValues.Values.ToArray();

            //entity.Connection.Delete(entity, fields, values);

            foreach (Entity foreignEntity in OrmManager.OrderedEntities)
            {
                if (foreignEntity.Resource.ForeignEntityPrimaryFields.Keys.Contains(entity))
                {
                    IEnumerable foreginObjects = foreignEntity.Connection.LazyRead(foreignEntity, fields, values);
                    foreach (object foreignObject in foreginObjects)
                    {
                        Delete(foreignEntity, foreignObject);
                    }
                }
            }

            entity.Connection.Delete(entity, fields, values);
        }

        //internal void Delete(Entity entity, List<EntityField> fields, object[] values)
        //{
        //    foreach (Entity foreignEntity in OrmManager.OrderedEntities)
        //    {
        //        if (foreignEntity.Resource.ForeignEntityPrimaryFields.Keys.Contains(entity))
        //        {
        //            IEnumerable foreginObjects = LazyRead(foreignEntity, fields, values);
        //            foreach (object obj in foreginObjects)
        //            {
        //                Delete(foreignEntity, obj);
        //            }
        //        }
        //    }
        //}

        internal static Dictionary<EntityField, object> GetIds(Entity entity, object obj)
        {
            Dictionary<EntityField, object> primaryFieldsValues = new Dictionary<EntityField, object>();

            GetPrimaryFieldValues(entity, primaryFieldsValues, obj);

            return primaryFieldsValues;
        }

        internal static object[] GetPrimaryFieldValues(Entity entity, Dictionary<EntityField, object> dictionary)
        {
            List<object> primaryFieldValues = new List<object>();

            foreach (EntityField field in dictionary.Keys)
            {
                if (entity.Resource.Primary.Contains(field))
                {
                    primaryFieldValues.Add(dictionary[field]);
                }
            }

            return primaryFieldValues.ToArray();
        }

        public static object LazyReadObject(Entity entity, Dictionary<EntityField, object> dictionary)
        {
            object obj = Activator.CreateInstance(entity.Class);
            foreach (EntityField field in entity.All)
            {
                if (field.ManyToOne != null)
                {
                    object foreignObject = ReadForeignObject(field.ManyToOne.Entity, entity, obj, dictionary);
                    field.PropertyInfo.SetValue(obj, foreignObject);
                }
                else if (field.OneToMany != null)
                {
                    field.PropertyInfo.SetValue(obj, null);
                }
                else
                {
                    field.PropertyInfo.SetValue(obj, dictionary[field]);
                }
            }
            return obj;
        }

        public static object FetchObject(Entity entity, Dictionary<EntityField, object> dictionary)
        {
            object obj = Activator.CreateInstance(entity.Class);
            foreach (EntityField field in entity.All)
            {
                if (field.ManyToOne != null)
                {
                    InitializeForeign(field, obj, dictionary);
                }
                else if (field.OneToMany != null)
                {
                    InitializeCollection(entity, field, obj, dictionary);
                }
                else
                {
                    field.PropertyInfo.SetValue(obj, dictionary[field]);
                }
            }
            return obj;
        }

        public static object ReadObject(Entity entity, EntityField parentEntityField, object parentObject, Dictionary<EntityField, object> dictionary)
        {
            object obj = Activator.CreateInstance(entity.Class);
            foreach (EntityField field in entity.All)
            {
                if (field.ManyToOne != null)
                {
                    //if (field.Equals(parentEntityField.OneToMany))
                    //{
                    //    field.PropertyInfo.SetValue(obj, parentObject);
                    //}
                    if (field.ManyToOne.Fetch == FetchType.Eager)
                    {
                        InitializeForeign(field, obj, dictionary);
                    }
                    else
                    {
                        object foreignObject = ReadForeignObject(field.ManyToOne.Entity, entity, obj, dictionary);
                        field.PropertyInfo.SetValue(obj, foreignObject);
                    }
                }
                else if (field.OneToMany != null)
                {
                    if (field.OneToMany.Fetch == FetchType.Eager)
                    {
                        InitializeCollection(entity, field, obj, dictionary);
                    }
                    else
                    {
                        field.PropertyInfo.SetValue(obj, null);
                    }
                }
                else
                {
                    field.PropertyInfo.SetValue(obj, dictionary[field]);
                }
            }
            return obj;
        }

        internal static void InitializeForeign(EntityField field, object obj, Dictionary<EntityField, object> dictionary)
        {
            Entity foreignEntity = field.ManyToOne.Entity;

            Type connectionType = foreignEntity.Connection.GetType();

            MethodInfo readMethodInfo = connectionType.GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(foreignEntity.Class);
            object foreignObject = readMethodInfo.Invoke(foreignEntity.Connection, new object[] { foreignEntity, GetPrimaryFieldValues(foreignEntity, dictionary) });

            field.PropertyInfo.SetValue(obj, foreignObject);
        }

        internal static void InitializeCollection(Entity entity, EntityField field, object obj, Dictionary<EntityField, object> dictionary)
        {
            Entity foreignEntity = field.OneToMany.Entity;
            IEnumerable<object> foreignObjects =
                foreignEntity.Connection.ReadForeign(foreignEntity, field, obj, foreignEntity.Resource.ForeignEntityPrimaryFields[entity], GetPrimaryFieldValues(entity, dictionary));

            Type objectListType = typeof(List<>).MakeGenericType(foreignEntity.Class);
            object list = Activator.CreateInstance(objectListType);

            foreach (object foreignObject in foreignObjects)
            {
                list.GetType().GetMethod("Add").Invoke(list, new[] { foreignObject });
            }

            field.PropertyInfo.SetValue(obj, list);
        }

        public static object ReadForeignObject(Entity entity, Entity parentEntity, object parentObject, Dictionary<EntityField, object> dictionary)
        {
            object obj = Activator.CreateInstance(entity.Class);
            foreach (EntityField field in entity.Primary)
            {
                if (field.ManyToOne != null)
                {
                    if (field.ManyToOne.Entity != parentEntity)
                    {
                        object foreignObject = ReadForeignObject(field.ManyToOne.Entity, parentEntity, parentObject, dictionary);
                        field.PropertyInfo.SetValue(obj, foreignObject);
                    }
                    else
                    {
                        field.PropertyInfo.SetValue(obj, parentObject);
                    }
                }
                else
                {
                    field.PropertyInfo.SetValue(obj, dictionary[field]);
                }
            }
            return obj;
        }

        public static void GetAllValues(Entity entity, Dictionary<EntityField, object> dictionary, object obj)
        {
            foreach (EntityField field in entity.All.Except(entity.OneToMany))
            {
                object fieldValue = field.PropertyInfo.GetValue(obj);
                if (field.ManyToOne == null)
                {
                    if (field.PrimaryKey != null && fieldValue == null)
                        throw new PrimaryKeyException();

                    dictionary.Add(field, fieldValue);
                }
                else
                {
                    if (fieldValue == null)
                        throw new ForeignKeyException();

                    GetForeignValues(field, dictionary, fieldValue);
                }
            }
        }

        public static void GetForeignValues(EntityField parentField, Dictionary<EntityField, object> dictionary, object obj)
        {
            foreach (EntityField field in parentField.ManyToOne.Entity.All.Except(parentField.ManyToOne.Entity.OneToMany))
            {
                object fieldValue = field.PropertyInfo.GetValue(obj);

                if (field.ManyToOne == null)
                {
                    dictionary.Add(field, fieldValue);
                }
                else
                {
                    GetForeignValues(field, dictionary, fieldValue);
                }
            }
            Entity parentEntity = parentField.ManyToOne.Entity;
            parentEntity.Connection.InsertUpdate(parentEntity, obj, dictionary);
        }

        public static void GetPrimaryFieldValues(Entity entity, Dictionary<EntityField, object> dictionary, object obj)
        {
            foreach (EntityField field in entity.Primary)
            {
                object fieldValue = field.PropertyInfo.GetValue(obj);
                if (field.ManyToOne == null)
                {
                    if (field.PrimaryKey != null && fieldValue == null)
                        throw new PrimaryKeyException();

                    dictionary.Add(field, fieldValue);
                }
                else
                {
                    if (fieldValue == null)
                        throw new ForeignKeyException();

                    GetPrimaryFieldValues(field.ManyToOne.Entity, dictionary, fieldValue);
                }
            }
        }
    }
}
