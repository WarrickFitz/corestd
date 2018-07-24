using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EOSLogParserConsole
{
    class ProductionHistory
    {
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        DateTime startingRound = DateTime.Now;
        Dictionary<DateTime, ProductionRound> history { get; set; } = new Dictionary<DateTime, ProductionRound>();
        public ProductionHistory()
        {
            history.Add(startingRound, new ProductionRound(startingRound));
        }

        public void updateHistory(lineParser line)
        {

            DateTime roundDateTimeToUpdate = DateTime.MinValue;
            //If max is not found yet then we know which ProductionRound to use
            if (history[startingRound].maxTimeSlot == DateTime.MaxValue)
            {
                logger.Debug("Block {0} arrived. We have not found the 1st round min and max yet.", line.blockNumber);
                roundDateTimeToUpdate = startingRound;
                history[roundDateTimeToUpdate].updateRound(line);
            }
            else
            {
                //This is block 
                if (line.signDateTime > maxTimeSlot)
                {
                    var newMin = maxTimeSlot.AddSeconds(0.5);
                    var newMax = maxTimeSlot.AddSeconds(0.5 + (252 * 0.5));

                    logger.Debug("Block {0} arrived. The maxTimeSlot that we have in any round is {1}. Create a new round {2}->{3}", line.blockNumber, maxTimeSlot, newMin, newMax);

                    history[getBlockRound(maxTimeSlot)].print();

                    history.Add(line.signDateTime, new ProductionRound(line.signDateTime, newMin, newMax));
                }
                else
                {
                    logger.Debug("Block {0} arrived. Looking up the Date of the Round we want to add it to: {1} ", line.blockNumber, roundDateTimeToUpdate);
                    roundDateTimeToUpdate = getBlockRound(line.signDateTime);

                    if (roundDateTimeToUpdate == DateTime.MinValue)
                    {

                        logger.Warn("Drop the record ... we don't have a slot for it.");
                    }
                    else
                    {
                        logger.Debug("Block {0} arrived. Add it to the round: {1} ", line.blockNumber, roundDateTimeToUpdate);
                        history[roundDateTimeToUpdate].updateRound(line);
                    }

                }

            }


        }

        /*
        public Int64 maxBlock
        {
            get
            {
                Int64 max = history.Values.OrderByDescending(i => i.maxBlock).FirstOrDefault().maxBlock;
                return max;
            }
        }
        */
        public DateTime maxTimeSlot
        {
            get
            {
                DateTime max = history.Values.OrderByDescending(i => i.maxTimeSlot).FirstOrDefault().maxTimeSlot;
                return max;
            }
        }


        DateTime getBlockRound(DateTime blockTime)
        {
            DateTime timeSlot = DateTime.MinValue;
            if (history[startingRound].maxTimeSlot < DateTime.MaxValue)
            {
                var orderedRounds = history.Keys.OrderByDescending(i => i);
                foreach (var dt in orderedRounds)
                {
                    var round = history[dt];
                    logger.Debug("Block {0} arrived. Check round {1} to see if it falls between {2} and {3}", blockTime, dt, round.minTimeSlot, round.maxTimeSlot);

                    if (blockTime <= round.maxTimeSlot && blockTime >= round.minTimeSlot)
                    {
                        logger.Debug("Block {0} arrived. Should be added to round {1}", blockTime, dt);
                        timeSlot = dt;
                    }
                }
            }
            else
            {
                logger.Debug("Block {0} arrived. Return min data as have not found a min/max yet", blockTime);
            }
            return timeSlot;
        }
    }
}
