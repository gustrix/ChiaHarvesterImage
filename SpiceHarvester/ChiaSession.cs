using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceHarvester
{
    class ChiaSession
    {
        public struct HarvestStats
        {
            public double worst;
            public double best;
            public double avg;
            public int proofs;
            public double blocks;

            public int totalChallenges;

            public double efficiency;
        }

        MBlock lastBlock = null;
        MBlock.Builder db;
        DateTime present;
        HarvestStats st;

        public List<string> warnings = new List<string>();
        public List<string> warnings_noHarvest = new List<string>();

        public ChiaSession(IEnumerator<string> Enr)
        {
            db = new MBlock.Builder(Enr);
            present = new DateTime(1900, 1, 1);

            st.totalChallenges = 0;
            st.efficiency = st.avg = st.worst = 0;
            st.blocks = 0;
            st.best = 999;

            // Throw away the first block, which may
            // be incomplete.
            db.GetBlock();
        }

        // Do final calculations
        public void End()
        {
            if (st.totalChallenges > 0)
            {
                st.efficiency /= (st.blocks * 6.4f);
                st.avg = st.avg / st.totalChallenges;
            }
            else st.best = 0;
        }

        public HarvestStats Stats { get { return st; } }

        public byte GetBlockValue()
        {
            // Get the next set of data
            if (lastBlock == null)
                lastBlock = db.GetBlock();

            MBlock block = lastBlock;

            if (block != null)
            {
                // Set to first timestamp in log
                if (present.Year == 1900)
                    present = block.dt;

                bool match = true;

                if (present.Year == block.dt.Year
                    && present.Month == block.dt.Month
                    && present.Day == block.dt.Day
                    && present.Hour == block.dt.Hour
                    && present.Minute != block.dt.Minute)
                {
                    match = false;
                }

                // Advance time
                present = present.AddMinutes(-1);
                st.blocks++;

                // No harvesting, keep the current block
                if (!match)
                {
                    warnings_noHarvest.Add(string.Format("No harvesting at {0}", present));
                    return 0;
                }

                int proofs = 0;
                byte cc = (byte)block.challenges.Count;
                // Clamp the color
                if (cc > 8) cc = 8;

                if (block.challenges.Count > 9)
                {
                    warnings.Add(string.Format("M-block found with {0} challenges.", block.challenges.Count));
                    warnings.Add(string.Format("   Timestamp: {0}", block.dt));
                    warnings.Add(string.Format("   Ignoring this M-block"));
                    
                    // Ignore this block
                    st.blocks--;
                    
                    // Use "no data" color
                    cc = 10;
                }
                else
                {
                    // Build our data based on challenges
                    foreach (Challenge cur in block.challenges)
                    {
                        st.totalChallenges++;
                        st.efficiency++;

                        st.avg += cur.time;
                        st.proofs += cur.proofs;
                        proofs += cur.proofs;

                        if (cur.time < st.best) st.best = cur.time;
                        if (cur.time > st.worst) st.worst = cur.time;
                    }
                }

                // Done with this block
                lastBlock = null;

                // If we had proofs, return that color instead
                if (proofs > 0) cc = 9;

                return cc;
            }

            // When we run out of data, use color 10
            return 10;
        }
    }
}
