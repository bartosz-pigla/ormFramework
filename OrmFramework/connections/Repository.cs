using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmFramework.exceptions;
using MySql.Data.MySqlClient;
using OrmFramework.core;
using OrmFramework.enums;

namespace OrmFramework.connections
{
    public class Repository<T>
    {
        internal static Entity Entity;

        public static Predicate<T> DefaultPredicate = x => true;

        static Repository()
        {
            Entity entity = OrmManager.GetEntity(typeof(T));
            if (entity == null)
                throw new TableNotFoundException();

            Entity = entity;
        }

        public static IEnumerable<T> GetIterator(int bufferSize, Predicate<T> predicate)
        {
            return Entity.Connection.GetIterator(bufferSize, predicate);
        }

        public static List<T>ReadRange(int pageNumber, int rowNumber)
        {
            List<T> objects=new List<T>(rowNumber);
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();

                foreach(object obj in Entity.Connection.Read(pageNumber, rowNumber, Entity))
                {
                    objects.Add((T)obj);
                }

                Entity.Connection.Disconnect();
            }
            return objects;
        }
        public static T Read(params object[] ids)
        {
            if (Entity.Resource.Primary.Count != ids.Length)
            {
                throw new PrimaryKeyException();
            }

            T obj;
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();

                obj=(T)Entity.Connection.Read(Entity, ids);

                Entity.Connection.Disconnect();
            }
            return obj;
        }

        public static List<T> Read(params T[] objects)
        {
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();

                List<T> fetchedObjects = new List<T>(objects.Length);
                foreach (T obj in objects)
                {
                    object[] ids = Connection.GetIds(Entity, obj).Values.ToArray();
                    fetchedObjects.Add((T)Entity.Connection.Fetch(Entity,ids));
                }

                Entity.Connection.Disconnect();

                return fetchedObjects;                
            }
        }

        public static void Delete(params T[] objects)
        {
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();
                foreach (T obj in objects)
                {
                    //Dictionary<EntityField,object> fieldValues= Connection.GetIds(Entity, obj);
                    //List<EntityField> fields = fieldValues.Keys.ToList();
                    //object[] values = fieldValues.Values.ToArray();

                    Connection.Delete(Entity, obj);
                }

                Entity.Connection.Disconnect();
            }
        }

        public static void Update(params T[] objects)
        {
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();

                foreach (object obj in objects)
                {
                    Entity.Connection.InsertUpdate(Entity, DatabaseOperation.UPDATE, obj);
                }

                Entity.Connection.Disconnect();
            }
        }

        public static void Save(params T[] objects)
        {
            lock (Entity.Connection)
            {
                Entity.Connection.Connect();

                foreach (object obj in objects)
                {
                    Entity.Connection.InsertUpdate(Entity, DatabaseOperation.INSERT, obj);
                }

                Entity.Connection.Disconnect();
            }
        }
    }
}
