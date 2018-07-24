using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace EOSNewYork.NodeosLogParser
{
    public class lineParser
    {

        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public string part_action { get; set; }
        public string part_blockInfo { get; set; }
        public string part_blockNumber { get; set; }
        public Int64 blockNumber
        {
            get
            {
                return Int64.Parse(part_blockNumber);
            }
        }
        public string part_signDateTime { get; set; }
        public DateTime signDateTime
        {
            get
            {
                return DateTime.SpecifyKind((DateTime.Parse(part_signDateTime)), DateTimeKind.Utc);
            }
        }
        public string part_producer { get; set; }

        public lineParser(string ln)
        {
            var lineParts = ln.Split('@');
            var prefixParts = lineParts[0].Trim().Split(' ');

            part_blockNumber = prefixParts[prefixParts.Length - 1].Trim().Replace("#", "");

            var signParts = lineParts[1].Split('[');
            var signByParts = signParts[0].Trim().Split(' ');

            part_signDateTime = signByParts[0];
            part_producer = signByParts[signByParts.Length - 1];

        }

        public void print()
        {
            logger.Info("{0}\t{1}\t{2}", signDateTime.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss.fffffffK"), part_producer, blockNumber);
        }
    }
}
