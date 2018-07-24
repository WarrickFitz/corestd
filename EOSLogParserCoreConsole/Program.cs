using System;
using System.Collections.Concurrent;
using System.Threading;
using EOSNewYork.NodeosLogParser;
using NLog;

namespace EOSLogParserCoreConsole
{
    class Program
    {
        public static ConcurrentQueue<String> logQ = null;
        public static ProductionHistory history = null;
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            history = new ProductionHistory();

            var LOGHOST = Environment.GetEnvironmentVariable("loghost");
            //Start the log collector, this strats a thread the feeds logs into the quete to be processed. 
            //LogsFromWebsocket logSource = new LogsFromWebsocket("ws://pennstation.eosdocs.io:8001/");
            LogsFromWebsocket logSource = new LogsFromWebsocket(LOGHOST);
            logSource.start();

            //Then start a thread that pulls log items from the queue and feeds them into the production history. 
            Thread thread = new Thread(new ThreadStart(WorkThreadFunction));
            thread.Start();
            logQ = LogsFromWebsocket.logQ;

            Console.ReadLine();
        }

        static void processLine(string s)
        {
            lineParser lineObj = new lineParser(s);
            lineObj.print();
            history.updateHistory(lineObj);
            //state.print(lineObj.part_producer);           
        }


        public static void WorkThreadFunction()
        {
            try
            {
                while (1 < 2)
                {
                    // I'm not sure why, but if you try to access the queue to quickly the application hangs. 
                    Thread.Sleep(1000);
                    while (logQ.Count > 0)
                    {
                        String result = String.Empty;
                        var qItem = logQ.TryDequeue(out result);
                        if (result.Contains("on_incoming_block") || result.Contains("produce_block"))
                        {
                            processLine(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }




    }
}
