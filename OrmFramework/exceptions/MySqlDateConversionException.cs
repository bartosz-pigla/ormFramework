using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.exceptions
{
    class MySqlDateConversionException:Exception
    {
        public MySqlDateConversionException(string type)
        {

        }

        public MySqlDateConversionException(Type type)
        {

        }
    }
}
