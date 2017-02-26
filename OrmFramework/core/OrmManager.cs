using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrmFramework.attributes;
using OrmFramework.connections;

namespace OrmFramework.core
{
    public class OrmManager
    {
        private static OrmManager _ormManager;
        private static readonly object Padlock = new object();
        internal static HashSet<Entity> Entities { get; set; }
        internal static List<Entity> OrderedEntities { get; set; }

        internal static Assembly AssemblyToScan { get; private set; }

        //internal OrmManager(Assembly assembly)
        //{
        //    _assemblyToScan = assembly;
        //    Entities = new HashSet<Entity>();
        //    ScanAssemblyTypes();
        //}

        internal OrmManager(Assembly assembly)
        {
            AssemblyToScan = assembly;
            Entities=new HashSet<Entity>();
            ScanAssemblyTypes();

            foreach (Entity entity in Entities)
            {
                Entity.SetRelation(entity);
            }

            foreach(Entity entity in Entities)
            {
                entity.Resource.Convert();

            }
        }

        internal void CreateSchema()
        {
            OrderedEntities = Entities.Where(entity => entity.ManyToOne.Count == 0).ToList();

            while (OrderedEntities.Count != Entities.Count)
            {
                foreach (Entity entity in Entities.Except(OrderedEntities))
                {
                    if (entity.ManyToOne.All(field => OrderedEntities.Contains(field.ManyToOne.Entity)))
                        OrderedEntities.Add(entity);
                }
            }

            foreach (Entity entity in OrderedEntities)
            {
                lock (entity.Connection)
                {
                    entity.Connection.Connect();

                    entity.Connection.Create(entity);

                    entity.Connection.Disconnect();
                }

            }
        }

        public static void Start(string configurationFile)
        {
            lock (Padlock)
            {
                if (_ormManager == null)
                {
                    _ormManager = new OrmManager(Assembly.GetCallingAssembly());
                    ConnectionReader.SetConnection(AssemblyToScan, configurationFile);
                    _ormManager.CreateSchema();
                }
            }
        }

        private static void ScanAssemblyTypes()
        {
            foreach (Type classType in AssemblyToScan.GetTypes())
            {
                AddEntity(classType);
            }
        }

        private static Entity AddEntity(Type classType)
        {
            Entity entity = GetAssemblyEntity(classType);
            if (entity != null)
                Entities.Add(entity);
            return entity;
        }
        
        private static Entity GetAssemblyEntity(Type classType)
        {
            foreach (CustomAttributeData attribute in classType.CustomAttributes)
            {
                if (attribute.AttributeType == typeof(Table))
                {
                    return new Entity(classType);
                }
            }
            return null;
        }
        internal static Entity GetBaseEntity(Type type)
        {
            Type baseType = type.BaseType;
            Entity parent = GetEntity(baseType);

            if (parent == null)
            {
                return AddEntity(baseType);
            }
            else
            {
                return parent;
            }
        }

        internal static Entity GetEntity(Type type)
        {
            foreach(Entity entity in Entities)
            {
                if (entity.Class == type)
                    return entity;
            }
            return null;
        }

        internal static Entity GetEntity(string tableName)
        {
            foreach (Entity entity in Entities)
            {
                if (entity.Table.Name.Equals(tableName))
                    return entity;
            }
            return null;
        }
    }
}