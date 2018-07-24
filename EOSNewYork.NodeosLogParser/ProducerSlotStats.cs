using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOSNewYork.NodeosLogParser
{
    public class ProducerSlotStats
    {
        public int producerOrder { get; set; }
        public Int64 hightestBlock { get; set; } = 0;
        public Dictionary<DateTime, lineParser> slots { get; set; } = new Dictionary<DateTime, lineParser>();
        public Int64 maxBlock
        {
            get
            {
                Int64 max = slots.Values.OrderByDescending(i => i.blockNumber).FirstOrDefault().blockNumber;
                return max;
            }
        }
        public Int64 minBlock
        {
            get
            {
                Int64 min = slots.Values.OrderByDescending(i => i.blockNumber).LastOrDefault().blockNumber;
                return min;
            }
        }

        public DateTime maxTimeSlot
        {
            get
            {
                var max = slots.Values.OrderByDescending(i => i.signDateTime).FirstOrDefault().signDateTime;
                return max;
            }
        }
        public DateTime minTimeSlot
        {
            get
            {
                var min = slots.Values.OrderByDescending(i => i.signDateTime).LastOrDefault().signDateTime;
                return min;
            }
        }

        public ProducerSlotStats(int order)
        {
            producerOrder = order;
        }

        public void registerBlockProduction(lineParser lineObj)
        {
            slots.Add(lineObj.signDateTime, lineObj);
            if (lineObj.blockNumber > hightestBlock)
            {
                if ((lineObj.blockNumber - hightestBlock) > 12)
                {
                    //slots.Add(lineObj.signDateTime, new slotStats());
                }
                hightestBlock = lineObj.blockNumber;
            }
        }

        public bool validateSequentialBlocks()
        {
            bool sequential = false;
            if ((maxBlock - minBlock) == 11)
                sequential = true;

            return sequential;

        }
    }
}
