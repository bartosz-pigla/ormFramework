using OrmFramework.converters;
using OrmFramework.enums;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace OrmFramework.connections
{
    class MySqlDatabase : Connection
    {
        internal const int StringBuilderCapacity = 128;
        internal MySqlConnection Connection;
        internal MySqlTransaction Transaction;
        internal SchemaCreationPriority SchemaCreationPriority;
        internal Dictionary<Entity, MySqlCrudCommand> Commands;

        public MySqlDatabase(string connectionString, SchemaCreationPriority schemaCreationPriority)
        {
            Connection = new MySqlConnection();
            Connection.ConnectionString = connectionString;

            Commands = new Dictionary<Entity, MySqlCrudCommand>();
            SchemaCreationPriority = schemaCreationPriority;
        }

        internal override void Connect()
        {
            if (Connection.State == System.Data.ConnectionState.Closed)
            {
                Connection.Open();
                Transaction = Connection.BeginTransaction();
            }
        }
        internal override void Disconnect()
        {
            if (Connection.State == System.Data.ConnectionState.Open)
            {
                Transaction.Commit();
                Connection.Close();
                Transaction = null;
            }

        }

        internal MySqlCrudCommand GetMySqlCrudCommand(Entity entity)
        {
            MySqlCrudCommand command;

            if (!Commands.TryGetValue(entity, out command))
            {
                command = new MySqlCrudCommand(entity, this);
                Commands.Add(entity, command);
            }

            return command;
        }

        internal override void DropTable(Entity entity)
        {
            CurrentOperation = DatabaseOperation.DROPTABLE;

            MySqlCommand createCommand = GetMySqlCrudCommand(entity).DropTableCommand;
            createCommand.Transaction = Transaction;

            createCommand.ExecuteNonQuery();
        }

        internal override void Create(Entity entity)
        {
            CurrentOperation = DatabaseOperation.CREATE;

            MySqlCrudCommand command = GetMySqlCrudCommand(entity);
            //if (command.MySqlConnection.SchemaCreationPriority == SchemaCreationPriority.FrameworkFirst)
            //{
            //    MySqlCommand dropTableCommand = command.DropTableCommand;
            //    dropTableCommand.Transaction = Transaction;
            //    dropTableCommand.ExecuteNonQuery();
            //}

            MySqlCommand createCommand = command.CreateCommand;
            createCommand.Transaction = Transaction;
            createCommand.ExecuteNonQuery();
        }


        internal override void InsertUpdate(Entity entity, object obj, Dictionary<EntityField, object> fieldValues)
        {
            MySqlCommand command;

            if (CurrentOperation == DatabaseOperation.INSERT)
                command = GetMySqlCrudCommand(entity).InsertCommand;
            else
                command = GetMySqlCrudCommand(entity).UpdateCommand;

            command.Transaction = Transaction;

            for (int i = 0; i < entity.Resource.All.Count; i++)
            {
                command.Parameters[i].Value = fieldValues[entity.Resource.All[i]];
            }

            Console.WriteLine(entity.Table.Name);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Transaction.Rollback();
            }
        }

        internal override void InsertUpdate(Entity entity, DatabaseOperation operation, params object[] objects)
        {
            CurrentOperation = operation;
            Dictionary<EntityField, object> fieldValues = new Dictionary<EntityField, object>();
            foreach (object obj in objects)
            {
                GetAllValues(entity, fieldValues, obj);
                InsertUpdate(entity, obj, fieldValues);
            }
        }

        internal Dictionary<EntityField,object> GetObjectValues(Entity entity, object[] ids)
        {
            CurrentOperation = DatabaseOperation.SELECT;
            MySqlCommand selectCommand = GetMySqlCrudCommand(entity).SelectCommand;
            selectCommand.Transaction = Transaction;
            Dictionary<EntityField, object> fieldValues = new Dictionary<EntityField, object>();

            for (int i = 0; i < entity.Resource.Primary.Count; i++)
            {
                selectCommand.Parameters[i].Value = ids[i];
            }
            MySqlDataReader reader = null; ;
            try
            {
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    for (int i = 0; i < entity.Resource.All.Count; i++)
                    {
                        EntityField field = entity.Resource.All[i];
                        object value = reader.GetValue(i);
                        fieldValues.Add(field, value);
                    }
                }

                reader.Close();
            }
            catch (Exception exc)
            {
                reader.Close();
                Transaction.Rollback();
            }
            return fieldValues;
        }

        internal override IEnumerable Read(int pageNumber, int rowNumber,Entity entity)
        {
            CurrentOperation = DatabaseOperation.SELECT_PAGINATION;
            MySqlCommand selectPaginationCommand = GetMySqlCrudCommand(entity).SelectPaginationCommand;
            selectPaginationCommand.Transaction = Transaction;

            selectPaginationCommand.Parameters[0].Value = pageNumber * rowNumber - rowNumber;
            selectPaginationCommand.Parameters[1].Value = rowNumber;

            MySqlDataReader reader = null;

            try
            {
                reader = selectPaginationCommand.ExecuteReader();

            }
            catch (Exception exc)
            {
                reader.Close();
                Transaction.Rollback();
            }

            List<Dictionary<EntityField, object>> fieldValueList = new List<Dictionary<EntityField, object>>();

            while (reader.Read())
            {
                Dictionary<EntityField, object> fieldValues = ReadRow(entity, reader);
                fieldValueList.Add(fieldValues);
            }
            reader.Close();

            foreach (Dictionary<EntityField, object> fieldValues in fieldValueList)
            {
                yield return ReadObject(entity, null, null, fieldValues);
            }
        }

        internal Dictionary<EntityField, object> ReadRow(Entity entity, MySqlDataReader reader)
        {
            Dictionary<EntityField, object> fieldValues = new Dictionary<EntityField, object>();
            for (int i = 0; i < entity.Resource.All.Count; i++)
            {
                EntityField field = entity.Resource.All[i];
                object value = reader.GetValue(i);
                fieldValues[field] = MySqlTypeConverter.Convert(field, value);
            }
            return fieldValues;
        }

        internal override object Read(Entity entity, params object[] ids)
        {
            Dictionary<EntityField, object> fieldValues = GetObjectValues(entity, ids);

            return ReadObject(entity,null,null, fieldValues);
        }

        internal override IEnumerable LazyRead(Entity entity, List<EntityField> fields, object[] values)
        {
            List<Dictionary<EntityField, object>> attributesOfObjects = GetAttributesOfObjects(entity, fields, values);

            foreach (Dictionary<EntityField, object> fieldValues in attributesOfObjects)
            {
                yield return LazyReadObject(entity, fieldValues);
            }
        }

        internal override object Fetch(Entity entity, params object[] ids)
        {
            Dictionary<EntityField, object> fieldValues = GetObjectValues(entity, ids);

            return FetchObject(entity, fieldValues);
        }

        internal override IEnumerable<T> Query<T>(string query)
        {
            throw new NotImplementedException();
        }

        internal List<Dictionary<EntityField, object>> GetAttributesOfObjects(Entity entity, List<EntityField> fields, object[] values)
        {
            CurrentOperation = DatabaseOperation.SELECT;
            MySqlCommand command = GetMySqlCrudCommand(entity).GetSelectCommand(fields);
            command.Transaction = Transaction;

            for (int i = 0; i < values.Length; i++)
            {
                command.Parameters[i].Value = MySqlTypeConverter.Convert(fields[i], values[i]);
            }

            MySqlDataReader reader = null;

            try
            {
                reader = command.ExecuteReader();

            }
            catch (Exception exc)
            {
                reader.Close();
                Transaction.Rollback();
            }

            List<Dictionary<EntityField, object>> attributesOfObjects = new List<Dictionary<EntityField, object>>();

            while (reader.Read())
            {
                Dictionary<EntityField, object> objectFieldValueList= ReadRow(entity, reader);
                attributesOfObjects.Add(objectFieldValueList);
            }
            reader.Close();

            return attributesOfObjects;
        }

        internal override IEnumerable<object> ReadForeign(Entity entity, EntityField parentEntityField, object parentObject, List<EntityField> foreignFields, object[] foreignIds)
        {
            List<Dictionary<EntityField, object>> attributesOfObjects = GetAttributesOfObjects(entity, foreignFields, foreignIds);

            foreach (Dictionary<EntityField, object> fieldValues in attributesOfObjects)
            {
                yield return ReadObject(entity, parentEntityField, parentObject, fieldValues);
            }
            //MySqlCommand command = GetMySqlCrudCommand(entity).GetSelectCommand(foreignFields);
            //command.Transaction = Transaction;

            //for (int i = 0; i < foreignFields.Count; i++)
            //{
            //    command.Parameters[i].Value = foreignIds[i];
            //}

            //MySqlDataReader reader = null;

            //try
            //{
            //    reader = command.ExecuteReader();

            //}
            //catch (Exception exc)
            //{
            //    Transaction.Rollback();
            //}

            //List<Dictionary<EntityField, object>> fieldValueList = new List<Dictionary<EntityField, object>>();

            //while (reader.Read())
            //{
            //    Dictionary<EntityField, object> fieldValues = ReadRow(entity, reader);
            //    fieldValueList.Add(fieldValues);
            //}
            //reader.Close();

            //foreach (Dictionary<EntityField, object> fieldValues in fieldValueList)
            //{
            //    yield return ReadObject(entity, parentEntityField, parentObject, fieldValues);
            //}
        }

        internal override IEnumerable<T> GetIterator<T>(int bufferSize, Predicate<T> predicate)
        {
            return new MySqlIterator<T>(bufferSize, predicate);
        }

        internal override void Delete(Entity entity, List<EntityField> fields, object[] values)
        {
            CurrentOperation = DatabaseOperation.DELETE;
            MySqlCommand command = GetMySqlCrudCommand(entity).GetDeleteCommand(fields);
            command.Transaction = Transaction;

            for (int i = 0; i < values.Length; i++)
            {
                command.Parameters[i].Value = MySqlTypeConverter.Convert(fields[i], values[i]);
            }

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Transaction.Rollback();
            }
        }
    }
}
