using MySql.Data.MySqlClient;
using OrmFramework.exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.converters
{
    class MySqlTypeConverter
    {
        internal static string ConvertToMySqlTypeAsString(EntityField field)
        {
            Type propertyType = field.PropertyInfo.PropertyType;
            if (propertyType == typeof(int))
            {
                return "int";
            }
            else if (propertyType == typeof(double))
            {
                return "DOUBLE";
            }
            else if (propertyType == typeof(DateTime))
            {
                return "DATETIME";
            }
            else if (propertyType == typeof(string))
            {
                return new StringBuilder("varchar(")
                    .Append(field.Column == null ? "255" : field.Column.Length.ToString())
                    .Append(")")
                    .ToString();
            }

            throw new MySqlDateConversionException(propertyType);
        }
        internal static MySqlDbType ConvertToMySqlTypeAsSqlDbType(EntityField field)
        {
            Type propertyType = field.PropertyInfo.PropertyType;
            if(propertyType == typeof(int))
            {
                return MySqlDbType.Int32;
            }
            else if (propertyType == typeof(double))
            {
                return MySqlDbType.Double;
            }
            else if (propertyType == typeof(DateTime))
            {
                return MySqlDbType.DateTime;
            }
            else if(propertyType == typeof(string))
            {
                return MySqlDbType.VarChar;
            }
            throw new MySqlDateConversionException(propertyType);
        }

        internal static object Convert(EntityField field, object value)
        {
            Type fieldType = field.PropertyInfo.PropertyType;

            return value;
            
            //if(fieldType == typeof(int))
            //{
            //    int value;
            //    Int32.TryParse()
            //}
        }

        //internal static Type ConvertToCSharpType(object obj)
        //{
        //    Type type = obj.GetType();

        //    type = type.Name.ToLower();

        //    if (type.Equals("int"))
        //        return typeof(int);
        //    else if (type.Equals("double"))
        //        return typeof(double);
        //    else if (type.Equals("datetime"))
        //        return typeof(DateTime);
        //    else if (type.StartsWith("varchar"))
        //        return typeof(string);

        //    throw new MySqlDateConversionException(type);
        //}
    }
}
