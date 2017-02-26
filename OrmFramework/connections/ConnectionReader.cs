using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using OrmFramework.core;
using OrmFramework.enums;
using OrmFramework.exceptions;

namespace OrmFramework.connections
{
    public class ConnectionReader
    {
        internal static void SetConnection(Assembly assembly, string filename)
        {
            UriBuilder uri = new UriBuilder(assembly.CodeBase);

            string directory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            directory = Directory.GetParent(Directory.GetParent(directory).ToString()).ToString();

            StringBuilder stringBuilder = new StringBuilder()
                .Append(directory)
                .Append("\\")
                .Append(string.Format("{0}", filename));

            directory = stringBuilder.ToString();

            try
            {
                using (XmlReader reader = XmlReader.Create(directory))
                {
                    Connection currentConnection = null;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.LocalName)
                            {
                                case "MySqlConnection":
                                    currentConnection = GetMySqlConnection(reader);
                                    break;
                                case "XmlConnection":
                                    currentConnection = GetXmlConnection(reader);
                                    break;
                                case "Table":
                                    Entity entity = OrmManager.GetEntity(reader.GetAttribute("name"));
                                    entity.Connection = currentConnection;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException exc)
            {
                throw new ConfigurationException(directory);
            }
            catch(Exception exc)
            {
                throw new ConfigurationException();
            }            
        }
        private static Connection GetXmlConnection(XmlReader reader)
        {
            string directory = reader.GetAttribute("directory");
            string priority = reader.GetAttribute("priority");

            SchemaCreationPriority schema = GetPriority(priority);

            return new XmlConnection(directory, schema);
        }

        private static Connection GetMySqlConnection(XmlReader reader)
        {
            string connectionString = reader.GetAttribute("connectionString");
            string priority = reader.GetAttribute("priority");

            SchemaCreationPriority schema = GetPriority(priority);

            return new MySqlDatabase(connectionString, schema);
        }

        private static SchemaCreationPriority GetPriority(string priority)
        {
            SchemaCreationPriority schema;

            switch (priority)
            {
                case "DatabaseFirst":
                    schema = SchemaCreationPriority.DatabaseFirst;
                    break;
                case "FrameworkFirst":
                    schema = SchemaCreationPriority.FrameworkFirst;
                    break;
                default:
                    throw new ConnectionCreationException("invalid connection mode");
            }
            return schema;
        }
    }
}
