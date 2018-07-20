using System;
using System.IO;
using System.Text;
using System.Threading;

namespace EOSLogParserConsole
{

    public class lineParser {


        public string part_dateTime { get; set; }
        public DateTime dateTime { get
            {
                return DateTime.SpecifyKind((DateTime.Parse(part_dateTime)),DateTimeKind.Utc);
            }
        }
        public string part_thread { get; set; }
        public string part_module { get; set; }
        public string part_action { get; set; }
        public string part_blockInfo { get; set; }
        public string part_blockNumber { get; set; }
        public Int64 blockNumber { get
            {
                return Int64.Parse(part_blockNumber);
            }
        }
        public string part_signDateTime { get; set; }
        public string part_producer { get; set; }

        public lineParser(string ln)
        {
            part_dateTime = ln.Substring(0, 23);
            part_thread = ln.Substring(24, 10);
            part_module = ln.Substring(35, 29);
            part_action = ln.Substring(65, 21);
            part_blockInfo = ln.Substring(88, 34);

            var remainder = ln.Substring(124, ln.Length - 124);
            var lineParts = remainder.Split("@");

            part_blockNumber = lineParts[0].Trim();

            var signParts = lineParts[1].Split("[");


            var signByParts = signParts[0].Trim().Split(" ");
            part_signDateTime = signByParts[0];
            part_producer = signByParts[signByParts.Length - 1];

        }

        public void print()
        {
            string.Format("{0}\t{1}\t{2}", dateTime.ToLocalTime(), part_producer, blockNumber);

            Console.WriteLine(string.Format("{0}\t{1}\t{2}", dateTime.ToLocalTime(), part_producer, blockNumber));
            /*
            Console.WriteLine(dateTime.ToLocalTime());
            Console.WriteLine(part_dateTime);
            Console.WriteLine(part_thread);
            Console.WriteLine(part_module);
            Console.WriteLine(part_action);
            Console.WriteLine(part_blockInfo);
            Console.WriteLine(blockNumber);
            Console.WriteLine(part_signDateTime);
            Console.WriteLine(part_producer);
            */
        }
    }





    class Program
    {
        static void Main(string[] args)
        {
            /*
            string ln = @"2018-07-20T18:19:42.500 thread-0   producer_plugin.cpp:1194      produce_block        ] Produced block 000047b9f6c393b2... #18361 @ 2018-07-20T18:19:42.500 signed by bp2 [trxs: 0, lib: 18347, confirmed: 0]";

            lineParser line = new lineParser(ln);
            line.print();
            */

            Console.ReadLine();
                string s;
                while ((s = Console.ReadLine()) != null)
                {
                    //Console.WriteLine(s);
                    lineParser line = new lineParser(s);
                    line.print();

                }


        }
    }
}
