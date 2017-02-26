using OrmFramework.converters;
using OrmFramework.enums;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.connections
{
    class MySqlConnection : Connection
    {
        internal const int StringBuilderCapacity = 128; 
        internal SqlConnection Connection;
        internal SchemaCreationPriority SchemaCreationPriority;
        internal Dictionary<Entity, MySqlCommand> Commands;
        public MySqlConnection(string connectionString, SchemaCreationPriority schemaCreationPriority)
        {
            //connection = new SqlConnection();
            //connection.ConnectionString = connectionString;
            Commands = new Dictionary<Entity, MySqlCommand>();
            SchemaCreationPriority = schemaCreationPriority;
        }

        internal override void Connect()
        {
            Connection.Open();
        }

        internal MySqlCommand GetCommand(Entity entity)
        {
            MySqlCommand command;

            if (! Commands.TryGetValue(entity,out command))
            {
                command = new MySqlCommand(entity, this);
                Commands.Add(entity, command);
            }

            return command;
        }

        internal override void Create(Entity entity)
        {
            SqlCommand createCommand = GetCommand(entity).CreateCommand;

            //Connect();

            //SqlCommand createCommand = GetCommand(entity).CreateCommand;
            //createCommand.ExecuteNonQuery();

            //Disconnect();
        }
        
        internal override void Delete(Entity entity, params object[] objects)
        {
            throw new NotImplementedException();
        }

        internal override void Disconnect()
        {
            Connection.Close();
        }

        internal override void Insert(Entity entity, params object[] objects)
        {
            Dictionary<EntityField, object> fieldValues = new Dictionary<EntityField, object>();

            for (int j=0;j<objects.Length;j++)
            {
                GetAllValues(entity, fieldValues, objects[j]);

                SqlCommand insertCommand = GetCommand(entity).InsertCommand;

                for (int i = 0; i < entity.Resource.All.Count; i++)
                {
                    insertCommand.Parameters[i].Value = fieldValues[entity.Resource.All[i]];
                }

                Console.WriteLine(insertCommand);

                //insertCommand.ExecuteNonQuery();
            }

            //Connect();

            //foreach(object obj in objects)
            //{
            //    GetAllValues(entity, fieldValues, obj);

            //    SqlCommand insertCommand = GetCommand(entity).InsertCommand;

            //    for (int i = 0; i < entity.Resource.All.Count; i++)
            //    {
            //        insertCommand.Parameters[i].Value = fieldValues[entity.Resource.All[i]];
            //    }

            //    insertCommand.ExecuteNonQuery();
            //}

            //Disconnect();
        }

        //internal StringBuilder GetRecord(Entity entity, StringBuilder insertCommand, Dictionary<EntityField,object> fieldValues, object obj)
        //{
        //    insertCommand
        //        .Append("(");
        //    foreach (EntityField field in entity.Resource.All)
        //    {
        //        insertCommand
        //            .Append("'")
        //            .Append(fieldValues[field].ToString())
        //            .Append("',");
        //    }

        //    insertCommand = CutLastComma(insertCommand);

        //    insertCommand
        //        .AppendLine("), ");

        //    return insertCommand;
        //}

        internal override T Read<T>(Entity entity, params object[] ids)
        {
            throw new NotImplementedException();
        }

        internal override void Update(Entity entity, params object[] objects)
        {
            throw new NotImplementedException();
        }

        internal override IEnumerable<T> Query<T>(string query)
        {
            throw new NotImplementedException();
        }




        //public object GetObject(EntityField field, Entity entity, object obj)
        //{

        //    if (entity == field.Owner)
        //        return field.PropertyInfo.GetValue(obj);
        //    else
        //    {
        //        GetObject(field, entity.Resource.Dictionary[field], entity.ManyToOne.First(f=>f.Owner == entity.Resource.Dictionary[field]).PropertyInfo.GetValue(obj));
        //    }
        //}

    }
}
