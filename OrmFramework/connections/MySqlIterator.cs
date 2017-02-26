using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmFramework.connections
{
    class MySqlIterator<T> : IEnumerable<T>, IEnumerator<T>
    {
        internal int Position;
        internal int Page;
        internal int BufferSize;
        internal int RowNumber;
        internal Predicate<T> Predicate;

        List<T> list;
        internal MySqlIterator(int bufferSize, Predicate<T> predicate)
        {
            Page = 0;
            Position = -1;
            RowNumber = 0;
            BufferSize = bufferSize;
            list = new List<T>(bufferSize);
            Predicate = predicate;
        }
        public T Current
        {
            get
            {
                return list[Position];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return list[Position];
            }
        }

        public void Dispose()
        {

        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            //Predicate.Invoke(Current);

            Position++;
            if (RowNumber <= Position + 1 && RowNumber % BufferSize==0)
            {
                ReadObjectFromDatabase();
            }
            if (RowNumber > Position)
            {
                if (Predicate.Invoke(Current))
                    return true;
                else
                    return MoveNext();
            }
            else
            {
                return false;
            }
        }

        internal void ReadObjectFromDatabase()
        {
            Page++;
            List<T> newList = Repository<T>.ReadRange(Page, BufferSize);
            RowNumber += newList.Count;
            list.AddRange(newList);
        }

        public void Reset()
        {
            Position = 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
