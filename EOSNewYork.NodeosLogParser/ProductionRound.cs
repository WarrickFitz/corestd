using EOSNewYork.EOSCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace EOSNewYork.NodeosLogParser
{
    public class ProductionRound
    {
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        Dictionary<string, ProducerSlotStats> producers = new Dictionary<string, ProducerSlotStats>();

        public DateTime ID { get; set; }
        //public Int64 maxBlock { get; set; } = Int64.MaxValue;
        //public Int64 minBlock { get; set; } = Int64.MinValue;

        public DateTime maxTimeSlot { get; set; } = DateTime.MaxValue;
        public DateTime minTimeSlot { get; set; } = DateTime.MinValue;

        public ProductionRound(DateTime roundID, DateTime min, DateTime max)
        {
            ID = roundID;
            minTimeSlot = min;
            maxTimeSlot = max;
            getProducerData();
        }

        public ProductionRound(DateTime roundID)
        {
            ID = roundID;
            getProducerData();
        }

        void getProducerData()
        {
            
            var HOST = Environment.GetEnvironmentVariable("apihost");
            var info = new EOS_Object<EOSProducerSchedule_row>(new Uri(HOST)).getAllObjectRecordsAsync().Result;
            int Count = 0;
            foreach (var producer in info.active.producers)
            {
                Count++;
                //logger.Info("{0}\t{1}", producer.producer_name, producer.block_signing_key);
                producers.Add(producer.producer_name, new ProducerSlotStats(Count));
            }
            
        }

        public void updateRound(lineParser line)
        {

            if (line.signDateTime > maxTimeSlot || line.signDateTime < minTimeSlot)
            {
                logger.Error("Round {0} has a min/max of {1}-{2}, block {3} should not be added here", ID, minTimeSlot, maxTimeSlot, line.blockNumber);
            }

            if (!producers.ContainsKey(line.part_producer))
            {
                logger.Warn("Round {0}, Producer {1} was not expected. Probably due to a schedule change", ID, line.part_producer);
                producers.Add(line.part_producer, new ProducerSlotStats(producers.Count + 1));
            }

            var producersSlotStats = producers[line.part_producer];
            if (producersSlotStats.slots.Count == 12)
            {
                logger.Error("Round {0}, Producer {1} already has 12 block .. this should not be possible", ID, line.part_producer);
            }



            producersSlotStats.registerBlockProduction(line);
            //This is the 1st round and we don't know what the min and max block for the round is yet. 
            if (maxTimeSlot == DateTime.MaxValue)
            {
                //As soon as we have 12 blocks from a producer and we know it's position in the production queue we can calc the rest
                if (producersSlotStats.slots.Count == 12 && producersSlotStats.validateSequentialBlocks())
                {
                    var order = producersSlotStats.producerOrder;
                    var totalBlocks = 12 * 21;
                    var blocksAhead = (21 - order) * 12;

                    maxTimeSlot = producersSlotStats.maxTimeSlot.AddSeconds(blocksAhead / 2);
                    minTimeSlot = maxTimeSlot.AddSeconds(-(totalBlocks / 2));

                    logger.Info("{0} at position {1} has a max block of {2}. There are {3} blocks remaining in this round", line.part_producer, order, producersSlotStats.maxTimeSlot, blocksAhead);
                    logger.Info("Round {0} was the 1st round. We calculated a min/max of {1}-{2}. Basis was producer {3} with a min/max of {4} {5}", ID, maxTimeSlot, minTimeSlot, line.part_producer, producersSlotStats.minTimeSlot, producersSlotStats.maxTimeSlot);
                }
            }
        }


        public void print()
        {

            logger.Info("Production Round {0}: Min {1}, Max {2}", ID, minTimeSlot, maxTimeSlot);

            foreach (var producer in producers)
            {
                if (producer.Value != null)
                {
                    if (producer.Value.slots.Count > 0)
                        logger.Info("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", producer.Key, producer.Value.producerOrder, producer.Value.slots.Count, producer.Value.minTimeSlot, producer.Value.maxTimeSlot, producer.Value.minBlock, producer.Value.maxBlock);
                    else
                        logger.Info("{0}\t{1}\t{2}\t{3}\t{4}", producer.Key, producer.Value.producerOrder, producer.Value.slots.Count, "-", "-");
                }
                else
                    logger.Info("{0}\t{1}\t{2}\t{3}\t{4}", producer.Key, producer.Value.producerOrder, producer.Value.slots.Count, "-", "-");
            }

        }
    }
}
