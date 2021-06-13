using System.IO;
using System;
using System.Drawing;

namespace SpiceHarvester
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("");
                Console.WriteLine("SpiceHarvester <debug.log> <image.png> [template.html]");
                Console.WriteLine("    Normally, stats will be sent to stdout but they can also be used with a template.");
                Console.WriteLine("    If a template is specified, then these placeholders will be replaced by values:");
                Console.WriteLine("        %img%         Your image filename"); 
                Console.WriteLine("        %best%        Best response time (ms)");
                Console.WriteLine("        %worst%       Worst response time (ms)");
                Console.WriteLine("        %avg%         The average response time (ms)");
                Console.WriteLine("        %efficiency%  How much harvesting was achieved");
                Console.WriteLine("        %proofs%      Number of proofs found");
                Console.WriteLine("");
                return 1;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found: {0}", args[0]);
                return -3;
            }

            // Open a reverse-reading stream
            var fr = new MiscUtil.IO.ReverseLineReader(args[0], System.Text.Encoding.ASCII);
            var enr = fr.GetEnumerator();

            // Create a session and iamger
            ChiaSession chs = new ChiaSession(enr);
            Imager img = new Imager(chs);

            // Process and render
            Bitmap b = img.RenderLog();

            fr = null;            

            try
            {
                b.Save(args[1]);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Could not save image: {0}", ex.Message);
                return -2;
            }

            chs.End();

            string st_bes = string.Format("{0:0.00}", chs.Stats.best * 1000);
            string st_wor = string.Format("{0:0.00}", chs.Stats.worst * 1000);
            string st_avg = string.Format("{0:0.00}", chs.Stats.avg * 1000);
            string st_eff = string.Format("{0:0.00}", chs.Stats.efficiency * 100);
            string st_pro = string.Format("{0}", chs.Stats.proofs);

            // Only output the template, if it's set
            if (args.Length == 3)
            {
                if (!File.Exists(args[2]))
                {
                    Console.WriteLine("\nError: Could not find file {0}.\n", args[2]);
                    return -1;
                }
                else
                {
                    string template = File.ReadAllText(args[2]);
                    template = template.Replace("%img%", args[1]);
                    template = template.Replace("%best%", st_bes);
                    template = template.Replace("%worst%", st_wor);
                    template = template.Replace("%avg%", st_avg);
                    template = template.Replace("%efficiency%", st_eff);
                    template = template.Replace("%proofs%", st_pro);

                    Console.WriteLine(template);
                    return 0;
                }
            }

            // Output standard info/stats

            foreach(string line in chs.warnings)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("");

            Console.WriteLine("Stats:\n");
            
            Console.WriteLine("img={0}", args[1]);
            Console.WriteLine("best={0}", st_bes);
            Console.WriteLine("worst={0}", st_wor);
            Console.WriteLine("avg={0}", st_avg);
            Console.WriteLine("efficiency={0}", st_eff);
            Console.WriteLine("proofs={0}", st_pro);

            Console.WriteLine("");

            return 0;
        }


    }
}
