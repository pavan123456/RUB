using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    using System;
    interface ICounter
    {
        void Increment();
    }


    struct Counter : ICounter
    {
        int value;
        public override string ToString()
        {
            return value.ToString();
        }
        void ICounter.Increment()
        {
            value++;
        }
    }



    class Program
    {
        static void Test<T>() where T : ICounter, new()
        {
            T x = new T();
            Console.WriteLine(x);
            x.Increment();
            // Modify x


            Console.WriteLine(x);

            ((ICounter)x).Increment();

            // Modify boxed copy of x

            Console.WriteLine(x);
            Console.ReadLine();
        }
        static void Main()
        {
            Test<Counter>();
        }
    }
}
