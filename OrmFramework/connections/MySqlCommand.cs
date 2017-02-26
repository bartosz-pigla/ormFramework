using OrmFramework.converters;
using OrmFramework.enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.connections
{
    class MySqlCommand
    {
        internal Entity Entity;
        internal MySqlConnection MySqlConnection;

        internal SqlCommand InsertCommand;
        internal SqlCommand CreateCommand;

        List<string> TableAttributeNames;

        internal MySqlCommand(Entity entity, MySqlConnection connection)
        {
            Entity = entity;
            MySqlConnection = connection;

            SetTableAttributeNames();

            SetInsertCommand();
            SetCreateCommand();
        }


        void SetCreateCommand()
        {
            StringBuilder stringBuilder = new StringBuilder(MySqlConnection.StringBuilderCapacity);
            stringBuilder
                .Append("CREATE TABLE ")
                .Append(MySqlConnection.SchemaCreationPriority == SchemaCreationPriority.DatabaseFirst ? "IF NOT EXISTS " : "")
                .Append(Entity.Table.Name)
                .AppendLine("(");

            foreach (EntityField field in Entity.Resource.All)
            {
                stringBuilder
                    .Append(field.Names[Entity])
                    .Append(" ")
                    .Append(MySqlTypeConverter.ConvertToMySqlTypeAsString(field))
                    .Append((field.NotNull != null || field.PrimaryKey != null) ? " NOT NULL," : ",")
                    .AppendLine();
            }

            stringBuilder
                .Append("PRIMARY KEY (");

            foreach (EntityField field in Entity.Resource.Primary)
            {
                stringBuilder
                    .Append(field.Names[Entity])
                    .Append(",");
            }

            stringBuilder = CutLastComma(stringBuilder);

            stringBuilder
                .Append("),")
                .AppendLine();

            foreach (Entity foreignEntity in Entity.Resource.Dictionary.Keys)
            {
                stringBuilder
                    .Append("FOREIGN KEY (");

                foreach (EntityField foreignField in Entity.Resource.Dictionary[foreignEntity])
                {
                    stringBuilder
                        .Append(foreignField.Names[Entity])
                        .Append(",");
                }

                stringBuilder = CutLastComma(stringBuilder);
                stringBuilder
                    .Append(") REFERENCES ")
                    .Append(foreignEntity.Table.Name)
                    .Append("(");

                foreach (EntityField foreignField in Entity.Resource.Dictionary[foreignEntity])
                {
                    stringBuilder
                        .Append(foreignField.Names[foreignEntity])
                        .Append(",");
                }

                stringBuilder = CutLastComma(stringBuilder);
                stringBuilder
                    .Append("),")
                    .AppendLine();
            }

            stringBuilder = CutLastComma(stringBuilder);

            stringBuilder.AppendLine(")");

            CreateCommand = new SqlCommand(stringBuilder.ToString(), MySqlConnection.Connection);
            CreateCommand.CommandType = CommandType.Text;
        }

        void SetInsertCommand()
        {
            List<string> parameterNames = new List<string>();

            StringBuilder insertCommand =
                new StringBuilder(MySqlConnection.StringBuilderCapacity)
                .Append("INSERT INTO ")
                .AppendLine(Entity.Table.Name)
                .Append("(");

            foreach (string attributeName in TableAttributeNames)
            {
                insertCommand
                    .Append(attributeName)
                    .Append(",");
            }
            
            insertCommand = CutLastComma(insertCommand);
            insertCommand
                .AppendLine(")")
                .AppendLine("VALUES (");

            foreach (string attributeName in TableAttributeNames)
            {
                string parameter = "@" + attributeName;
                parameterNames.Add(parameter);
                insertCommand
                    .Append(parameter)
                    .Append(",");
            }

            insertCommand = CutLastComma(insertCommand);
            insertCommand
                .Append(")");

            Console.WriteLine(insertCommand);

            InsertCommand = new SqlCommand(insertCommand.ToString(), MySqlConnection.Connection);

            for(int i=0; i < TableAttributeNames.Count; i++)
            {
                InsertCommand.Parameters.Add(parameterNames[i], MySqlTypeConverter.ConvertToMySqlTypeAsSqlDbType(Entity.Resource.All[i]));
            }

            InsertCommand.CommandType = CommandType.Text;
        }

        void SetTableAttributeNames()
        {
            TableAttributeNames = new List<string>();
            foreach (EntityField field in Entity.Resource.All)
            {
                if (field.ManyToOne == null)
                {
                    TableAttributeNames.Add(field.Names[Entity]);
                }
                else
                {
                    Entity foreignEntity = field.ManyToOne.Entity;
                    TableAttributeNames.Add(field.Names[foreignEntity]);
                }
            }
        }

        internal static StringBuilder CutLastComma(StringBuilder stringBuilder)
        {
            string query = stringBuilder.ToString();
            int lastCommaPosition = query.LastIndexOf(',');
            //if(lastCommaPosition!=-1)
            return new StringBuilder(query.Substring(0, lastCommaPosition));
        }
    }
}
