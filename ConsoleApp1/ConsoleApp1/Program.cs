using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFNToAFD
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var convert = new ConvertTo();

                convert.StartConvert();

                Console.Write("\nAperte qualquer letra para recomeçar.");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}
