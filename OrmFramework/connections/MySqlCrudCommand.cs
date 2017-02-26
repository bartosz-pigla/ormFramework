using MySql.Data.MySqlClient;
using OrmFramework.converters;
using OrmFramework.enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.connections
{
    class MySqlCrudCommand
    {
        internal Entity Entity;
        internal MySqlDatabase MySqlConnection;

        internal MySqlCommand InsertCommand;
        internal MySqlCommand CreateCommand;
        internal MySqlCommand UpdateCommand;
        internal MySqlCommand SelectCommand;
        internal MySqlCommand SelectPaginationCommand;
        internal MySqlCommand DeleteCommand;
        internal MySqlCommand DropTableCommand;

        internal Dictionary<List<EntityField>, MySqlCommand> SelectCommands;
        internal Dictionary<List<EntityField>, MySqlCommand> DeleteCommands;

        string[] TableAttributeNames;
        Dictionary<EntityField,string> ParameterNames;
        Dictionary<EntityField,MySqlParameter> Parameters;

        MySqlParameter FirstRowIndexParameter;
        MySqlParameter RowNumberParameter;

        internal MySqlCrudCommand(Entity entity, MySqlDatabase connection)
        {
            Entity = entity;
            MySqlConnection = connection;

            SelectCommands = new Dictionary<List<EntityField>, MySqlCommand>();
            DeleteCommands = new Dictionary<List<EntityField>, MySqlCommand>();

            SetCommandParameters();

            SetCreateCommand();
            SetInsertCommand();
            SetUpdateCommand();
            SetSelectCommand();
            SetSelectPaginationCommand();
            SetDropTableCommand();
        }

        internal MySqlCommand GetSelectCommand(List<EntityField> fields)
        {
            MySqlCommand command = GetExistingCommand(SelectCommands, fields);
            if (command == null)
            {
                StringBuilder selectCommandBuilder =
                    new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                    .Append("SELECT * FROM ")
                    .Append(Entity.Table.Name)
                    .Append(" WHERE ");

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                foreach (EntityField field in fields)
                {
                    parameters.Add(Parameters[field]);

                    selectCommandBuilder
                        .Append(field.Names[Entity])
                        .Append("=")
                        .Append(ParameterNames[field])
                        .Append(" AND ");
                }
                selectCommandBuilder = CutLast(selectCommandBuilder, "AND");

                command = GetCommand(selectCommandBuilder, parameters);

                SelectCommands.Add(fields, command);
            }
            return command;
        }

        internal MySqlCommand GetDeleteCommand(List<EntityField> fields)
        {
            MySqlCommand command = GetExistingCommand(DeleteCommands, fields);
            if (command == null)
            {
                StringBuilder deleteCommandBuilder =
                    new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                    .Append("DELETE FROM ")
                    .Append(Entity.Table.Name)
                    .Append(" WHERE ");

                List<MySqlParameter> parameters = new List<MySqlParameter>(fields.Count);

                foreach (EntityField field in fields)
                {
                    deleteCommandBuilder
                        .Append(field.Names[Entity])
                        .Append("=")
                        .Append(ParameterNames[field])
                        .Append(" AND ");

                    parameters.Add(Parameters[field]);
                }

                deleteCommandBuilder = CutLast(deleteCommandBuilder, "AND");

                command = GetCommand(deleteCommandBuilder, parameters);

                DeleteCommands.Add(fields, command);
            }
            return command;
        }

        void SetSelectPaginationCommand()
        {
            StringBuilder selectCommandBuilder =
                    new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                    .Append("SELECT * FROM ")
                    .Append(Entity.Table.Name)
                    .Append(" LIMIT @firstRowIndex, @rowNumber");

            SelectPaginationCommand = GetCommand(selectCommandBuilder, new MySqlParameter[] { FirstRowIndexParameter, RowNumberParameter });
        }

        void SetDropTableCommand()
        {
            StringBuilder dropTableCommandBuilder=new StringBuilder(MySqlDatabase.StringBuilderCapacity);
            dropTableCommandBuilder
                .Append("DROP TABLE ")
                .Append(Entity.Table.Name);

            DropTableCommand = new MySqlCommand(dropTableCommandBuilder.ToString(), MySqlConnection.Connection);
            DropTableCommand.CommandType = CommandType.Text;
        }

        void SetCreateCommand()
        {
            StringBuilder createCommandBuilder = new StringBuilder(MySqlDatabase.StringBuilderCapacity);
            createCommandBuilder
                .Append("CREATE TABLE ")
                .Append("IF NOT EXISTS ")
                .Append(Entity.Table.Name)
                .AppendLine("(");

            foreach (EntityField field in Entity.Resource.All)
            {
                createCommandBuilder
                    .Append(field.Names[Entity])
                    .Append(" ")
                    .Append(MySqlTypeConverter.ConvertToMySqlTypeAsString(field))
                    .Append((field.NotNull != null || field.PrimaryKey != null) ? " NOT NULL," : ",")
                    .AppendLine();
            }

            createCommandBuilder
                .Append("PRIMARY KEY (");

            foreach (EntityField field in Entity.Resource.Primary)
            {
                createCommandBuilder
                    .Append(field.Names[Entity])
                    .Append(",");
            }

            createCommandBuilder = CutLast(createCommandBuilder,",");

            createCommandBuilder
                .Append("),")
                .AppendLine();

            //foreach(Entity foreignEntity in Entity.Resource.ForeignEntityPrimaryFields.Keys)

            foreach (Entity foreignEntity in Entity.Resource.ForeignEntityPrimaryFields.Keys)
            {
                createCommandBuilder
                    .Append("FOREIGN KEY (");

                foreach (EntityField foreignField in Entity.Resource.ForeignEntityPrimaryFields[foreignEntity])
                {
                    createCommandBuilder
                        .Append(foreignField.Names[Entity])
                        .Append(",");
                }

                createCommandBuilder = CutLast(createCommandBuilder,",");
                createCommandBuilder
                    .Append(") REFERENCES ")
                    .Append(foreignEntity.Table.Name)
                    .Append("(");

                foreach (EntityField foreignField in Entity.Resource.ForeignEntityPrimaryFields[foreignEntity])
                {
                    createCommandBuilder
                        .Append(foreignField.Names[foreignEntity])
                        .Append(",");
                }

                createCommandBuilder = CutLast(createCommandBuilder, ",");
                createCommandBuilder
                    .Append("),")
                    .AppendLine();
            }

            createCommandBuilder = CutLast(createCommandBuilder, ",");

            createCommandBuilder.AppendLine(")");

            CreateCommand = new MySqlCommand(createCommandBuilder.ToString(), MySqlConnection.Connection);
            CreateCommand.CommandType = CommandType.Text;
        }

        void SetUpdateCommand()
        {
            StringBuilder updateCommandBuilder =
                new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                .Append("UPDATE ")
                .Append(Entity.Table.Name)
                .Append(" SET ");

            foreach(EntityField field in Entity.Resource.All)
            {
                updateCommandBuilder
                    .Append(field.Names[Entity])
                    .Append("=")
                    .Append(ParameterNames[field])
                    .Append(", ");
            }

            updateCommandBuilder = CutLast(updateCommandBuilder, ",");

            updateCommandBuilder
                .Append(" WHERE ");

            foreach(EntityField field in Entity.Resource.Primary)
            {
                updateCommandBuilder
                    .Append(field.Names[Entity])
                    .Append("=")
                    .Append(ParameterNames[field])
                    .Append(" AND ");
            }
            
            updateCommandBuilder = CutLast(updateCommandBuilder, "AND");

            Console.WriteLine(updateCommandBuilder);

            UpdateCommand = GetCommand(updateCommandBuilder, Parameters.Values);
        }

        void SetInsertCommand()
        {
            StringBuilder insertCommandBuilder =
                new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                .Append("INSERT IGNORE INTO ")
                .AppendLine(Entity.Table.Name)
                .Append("(");

            foreach (string attributeName in TableAttributeNames)
            {
                insertCommandBuilder
                    .Append(attributeName)
                    .Append(",");
            }

            insertCommandBuilder = CutLast(insertCommandBuilder,",");
            insertCommandBuilder
                .AppendLine(")")
                .AppendLine("VALUES (");

            foreach (string parameterName in ParameterNames.Values)
            {
                insertCommandBuilder
                    .Append(parameterName)
                    .Append(",");
            }

            insertCommandBuilder = CutLast(insertCommandBuilder, ",");
            insertCommandBuilder
                .Append(")");

            Console.WriteLine(insertCommandBuilder);

            InsertCommand = GetCommand(insertCommandBuilder, Parameters.Values);
        }

        void SetSelectCommand()
        {
            StringBuilder selectCommandBuilder =
                new StringBuilder(MySqlDatabase.StringBuilderCapacity)
                .Append("SELECT * FROM ")
                .Append(Entity.Table.Name)
                .Append(" WHERE ");

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            foreach(EntityField field in Entity.Resource.Primary)
            {
                parameters.Add(Parameters[field]);

                selectCommandBuilder
                    .Append(field.Names[Entity])
                    .Append("=")
                    .Append(ParameterNames[field])
                    .Append(" AND ");
            }
            selectCommandBuilder = CutLast(selectCommandBuilder, "AND");
            SelectCommand = GetCommand(selectCommandBuilder, parameters);
        }

        internal MySqlCommand GetExistingCommand(Dictionary<List<EntityField>,MySqlCommand> commands ,List<EntityField> fields)
        {
            foreach (List<EntityField> existingFields in commands.Keys)
            {
                if (existingFields.SequenceEqual(fields))
                {
                    return commands[existingFields];
                }
            }
            return null;
        }        

        MySqlCommand GetCommand(StringBuilder commandBuilder, IEnumerable<MySqlParameter> parameters)
        {
            MySqlCommand command = new MySqlCommand(commandBuilder.ToString(), MySqlConnection.Connection);

            foreach (MySqlParameter parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            command.CommandType = CommandType.Text;

            return command;
        }

        void SetCommandParameters()
        {
            int entityFieldsCount = Entity.Resource.All.Count;

            TableAttributeNames = new string[entityFieldsCount];
            ParameterNames = new Dictionary<EntityField, string>(entityFieldsCount);
            Parameters = new Dictionary<EntityField, MySqlParameter>();

            for (int i = 0; i < entityFieldsCount; i++)
            {
                string parameterName = string.Empty;
                EntityField field = Entity.Resource.All[i];
                if (field.ManyToOne == null)
                {
                    string name = field.Names[Entity];
                    TableAttributeNames[i] = name;
                    parameterName = "@" + name;
                    ParameterNames.Add(field, parameterName);
                }
                else
                {
                    Entity foreignEntity = field.ManyToOne.Entity;
                    string name = field.Names[foreignEntity];
                    TableAttributeNames[i] = name;
                    parameterName = "@" + name;
                    ParameterNames.Add(field, parameterName);
                }

                MySqlDbType type = MySqlTypeConverter.ConvertToMySqlTypeAsSqlDbType(Entity.Resource.All[i]);
                Parameters.Add(field, new MySqlParameter(parameterName, type));
            }

            FirstRowIndexParameter = new MySqlParameter("@firstRowIndex",MySqlDbType.Int32);
            RowNumberParameter = new MySqlParameter("@rowNumber", MySqlDbType.Int32);
        }

        internal static StringBuilder CutLast(StringBuilder stringBuilder, string keyword)
        {
            string query = stringBuilder.ToString();
            int lastCommaPosition = query.LastIndexOf(keyword);
            if (lastCommaPosition != -1)
                return new StringBuilder(query.Substring(0, lastCommaPosition));
            else
                return stringBuilder;
        }
    }
}
