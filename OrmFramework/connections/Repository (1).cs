using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmFramework.exceptions;

namespace OrmFramework.connections
{
    public class Repository<T>
    {
        internal static Entity Entity;

        static Repository()
        {
            Entity entity = OrmManager.GetEntity(typeof(T));
            if (entity == null)
                throw new TableNotFoundException();
            else
            {
                Entity = entity;
            }
        }

        public static void Save(params T[] objects)
        {

        }
    }
}
