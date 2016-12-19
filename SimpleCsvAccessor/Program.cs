using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SimpleCsvAccessor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fs = new FileStream("test.csv", FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var scr = new SimpleCsvReader(sr);
                    string[] line = null;
                    while(null != (line = scr.ReadRecord()))
                    {
                        PrintCSVLine(line);
                    }
                }
            }
        }

        static void PrintCSVLine(string[] line)
        {
            foreach (var item in line)
            {
                Console.Write(item);
                Console.Write("\t");
            }

            Console.WriteLine();
        }
    }
}
