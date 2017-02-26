using OrmFramework;
using OrmFramework.connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmFramework.core;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //List<>list=new List<>();
            //foreach(object foreignObject in foreignObjects)
            //{
            //    list.Add(foreignObject);
            //}
            //obj.field = list;

            OrmManager.Start("OrmConfig.xml");
            User u = new User();
            u.UserId = 1;
            u.FirstName = "imie1";

            Ord o1 = new Ord();
            o1.OrdId = 1;
            o1.User = u;
            o1.Name = "product1";

            Ord o2 = new Ord();
            o2.OrdId = 2;
            o2.User = u;
            o2.Name = "product2";

            Repository<Ord>.Save(o1);
            Repository<Ord>.Save(o2);

            Repository<User>.Save(u);

            Predicate<Ord> orderPredicate = ValidOrder;

            Console.WriteLine("OrmFramework: Zamowienia");

            foreach (Ord order in Repository<Ord>.GetIterator(10,orderPredicate))
            {
                Console.WriteLine(order.Name);
            }

            Console.ReadLine();
        }

        static bool ValidOrder(Ord order)
        {
            return order.Name.Equals("product1");
        }
    }
}
