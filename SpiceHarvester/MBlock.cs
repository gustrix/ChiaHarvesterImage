using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceHarvester
{
    /// <summary>
    /// A challenge
    /// </summary>
    class Challenge
    {
        public override string ToString()
        {
            return string.Format("Challenge {1}ms @ {0}", dt, time);
        }

        public double time = 0;
        public int proofs = 0;
        public DateTime dt = new DateTime(1900, 1, 1);
        public int checks = 0;

        public static byte[] colors =
        {
            0x71, 0x03, 0x01, // 0 (challenge count)
            0xC5, 0x21, 0x04,
            0xF2, 0x64, 0x05,
            0xF2, 0x96, 0x05,
            0xEF, 0xF2, 0x13,
            0x7D, 0xF2, 0x13,
            0x20, 0xEE, 0x41, // 6
            0x16, 0xE1, 0x36, // 7
            0x01, 0xBF, 0x31, // 8
            0xFF, 0xFF, 0xFF, // Proof
            0x3D, 0x3D, 0x3D, // No data
        };
    }

    /// <summary>
    /// A block of data, one per minute
    /// </summary>
    class MBlock
    {
        public DateTime dt;
        public List<Challenge> challenges = new List<Challenge>();
        public bool valid = false;

        public class Builder
        {
            Challenge next = null;
            bool eof = false;
            IEnumerator<string> enr;
            public Builder(IEnumerator<string> Enr)
            {
                if (Enr == null) throw new Exception("Invalid enumerator in builder");
                enr = Enr;
            }

            /// <summary>
            /// Parse a challenge
            /// </summary>
            /// <param name="Line"></param>
            /// <param name="Data"></param>
            /// <returns></returns>
            public bool Parse(string Line, out Challenge Data)
            {
                Data = null;

                // Validate string
                if (Line == null) return false;
                string l = Line.Trim();
                if (l == null) return false;
                if (l.Length < 10) return false;
                if (!l.Contains("eligible") || !l.Contains("proofs") || !l.Contains("Time:")) return false;

                while (l.Contains("  "))
                    l = l.Replace("  ", " ");

                // Split string into data
                string[] parts = l.Split(' ');
                if (parts.Length < 16) return false;

                // Object from data
                Data = new Challenge();
                Data.dt = DateTime.Parse(parts[0]);
                Data.proofs = int.Parse(parts[12]);
                Data.checks = int.Parse(parts[4]);
                Data.time = double.Parse(parts[15]);

                return true;
            }

            void ProcessChallenge(MBlock Target)
            {
                if (next == null) return;

                if (!Target.valid)
                {
                    Target.valid = true;
                    // Set time (we want the minute)
                    Target.dt = next.dt;
                }

                Target.challenges.Add(next);

                next = null;
            }

            public MBlock GetBlock()
            {
                MBlock block = new MBlock();
                ProcessChallenge(block);

                while(GetNextChallenge())
                {
                    if (block.valid)
                    {
                        if (next.dt.Year == block.dt.Year
                            && next.dt.Month == block.dt.Month
                            && next.dt.Day == block.dt.Day
                            && next.dt.Hour == block.dt.Hour
                            && next.dt.Minute == block.dt.Minute)
                        {
                            ProcessChallenge(block);
                        }
                        else
                            // Halt when the minute changes
                            break;
                    }
                    else
                        ProcessChallenge(block);
                }

                if (block.valid) return block;
                return null;
            }

            /// <summary>
            /// Read until EOF or we hit a challenge line
            /// </summary>
            /// <param name="Enr"></param>
            /// <returns></returns>
            bool GetNextChallenge()
            {
                if (eof) return false;

                bool found = false;

                while (!found)
                {
                    // Next line
                    if (!enr.MoveNext())
                    {
                        eof = true;
                        return false;
                    }

                    if (Parse(enr.Current, out next))
                        found = true;
                }

                return true;
            }
        }
    }
}
