using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MateralSE.ConsoleApp
{
    internal class Program
    {
        public static void Main()
        {
            var f = 0.3334444433f;
            Console.WriteLine($"{f*100:N2}%");
            Console.ReadKey();
        }
    }
}
