using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmFramework.enums;

namespace OrmFramework.connections
{
    class XmlConnection:Connection
    {
        internal SchemaCreationPriority SchemaCreationPriority;
        internal string directory;

        internal XmlConnection(string directory, SchemaCreationPriority schemaCreationPriority)
        {

        }

        internal override void Create(Entity entity)
        {
            throw new NotImplementedException();
        }

        internal override void DropTable(Entity entity)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable Read(int pageNumber, int rowNumber, Entity entity)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<T> GetIterator<T>(int bufferSize, Predicate<T> predicate)
        {
            throw new NotImplementedException();
        }

        internal override object Read(Entity entity, params object[] ids)
        {
            throw new NotImplementedException();
        }

        internal override object Fetch(Entity entity, params object[] ids)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable LazyRead(Entity entity, List<EntityField> fields, object[] values)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<object> ReadForeign(Entity entity, EntityField parentEntityField, object parentObject, List<EntityField> foreignFields, object[] foreignIds)
        {
            throw new NotImplementedException();
        }

        internal override void Delete(Entity entity, List<EntityField> fields, object[] values)
        {
            throw new NotImplementedException();
        }

        internal override void InsertUpdate(Entity entity, DatabaseOperation operation, params object[] objects)
        {
            throw new NotImplementedException();
        }

        internal override void InsertUpdate(Entity entity, object obj, Dictionary<EntityField, object> dictionary)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<T> Query<T>(string query)
        {
            throw new NotImplementedException();
        }

        internal override void Connect()
        {
            throw new NotImplementedException();
        }

        internal override void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
