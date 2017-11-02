using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fizz
{
    class Program
    {
        static void Main(string[] args)
        {
            int a = 0;
            int b = 0;

            bool flagA = false;
            bool flagB = false;

            for (int i = 0; i < 101; i++)
            {
                if (a == 0)
                {
                    flagA = true;
                    a++;
                }
                else if (a == 2)
                    a = 0;
                else
                    a++;

                if (b == 0)
                {
                    flagB = true;
                    b++;
                }
                else if (b == 4)
                    b = 0;
                else
                    b++;

                if (flagA && flagB)
                {
                    Console.Write("FizzBuzz");
                    flagA = false;
                    flagB = false;
                }
                else if (flagA)
                {
                    Console.Write("Fizz");
                    flagA = false;
                }
                else if (flagB)
                {
                    Console.Write("Buzz");
                    flagB = false;
                }
                else
                    Console.Write(i);

                Console.WriteLine();
            }

            Console.ReadLine();
        }

    }
}
