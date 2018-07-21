using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;

namespace EOSLogParserConsole
{

    public class producerState
    {
        Dictionary<string, producerStats> producers { get; set; } = new Dictionary<string, producerStats>();

        public void registerBlockProduction(lineParser lineObj)
        {
            var exists = producers.ContainsKey(lineObj.part_producer);
            if(!exists)
            {
                producers.Add(lineObj.part_producer, new producerStats());
            }
            producers[lineObj.part_producer].registerBlockProduction(lineObj);
        }

        public void print(String producer)
        {
            Console.WriteLine(string.Format("{0}\t{1}", producer, producers[producer].hightestBlock));
        }


    }

    public class producerStats
    {
        public Int64 hightestBlock { get; set; } = 0;
        Dictionary<DateTime, slotStats> slots { get; set; } = new Dictionary<DateTime, slotStats>();
        public void registerBlockProduction(lineParser lineObj)
        {
            if (lineObj.blockNumber > hightestBlock)
            {
                if((lineObj.blockNumber - hightestBlock) > 12)
                {
                    slots.Add(lineObj.signDateTime, new slotStats());
                }
                hightestBlock = lineObj.blockNumber;

            }
                
        }
    }

    public class slotStats
    {
        public int blockCount { get; set; }
        Dictionary<DateTime, producerStats> producers { get; set; } = new Dictionary<DateTime, producerStats>();
    }


    class Program
    {
        static producerState state = new producerState();
        public static ConcurrentQueue<String> logQ = null;



        static void Main(string[] args)
        {
            LogsFromWebsocket logSource = new LogsFromWebsocket("ws://badger.eosdocs.io:8080/");
            logSource.start();


            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();
            logQ = LogsFromWebsocket.logQ;

            Console.ReadLine();


            /*
            string ln = @"2018-07-20T18:19:42.500 thread-0   producer_plugin.cpp:1194      produce_block        ] Produced block 000047b9f6c393b2... #18361 @ 2018-07-20T18:19:42.500 signed by bp2 [trxs: 0, lib: 18347, confirmed: 0]";
            exec(ln);
            exec(ln);

            Console.ReadLine();
            string s;
            while ((s = Console.ReadLine()) != null)
            {
                exec(s);
            }
            */

    }

        static void processLine(string s)
        {
            //Console.WriteLine(s);
            lineParser lineObj = new lineParser(s);
            state.registerBlockProduction(lineObj);
            lineObj.print();
            //state.print(lineObj.part_producer);           
        }



        public static void WorkThreadFunction()
        {
            try
            {
                while(1<2)
                {
                    
                    // I'm not sure why, but if you try to access the queue to quickly the application hangs. 
                    Thread.Sleep(1000);
                    while(logQ.Count > 0)
                    {

                        String result = String.Empty;
                        var qItem = logQ.TryDequeue(out result);
                        if(result.Contains("on_incoming_block") || result.Contains("produce_block"))
                        {
                            processLine(result);
                        }
                        
                    }                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }




    }
}
