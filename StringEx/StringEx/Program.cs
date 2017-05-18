using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringEx
{
    class Program
    {
        static void Main(string[] args)
        {
            string temp1 = string.Empty;
            String  abc = "kasturi";

            temp1 = string.Concat(abc.Substring(1, abc.Length - 1), abc[0]);

            Console.WriteLine("xyz = {0}", temp1);
            Console.ReadLine();
        }
    }
}
