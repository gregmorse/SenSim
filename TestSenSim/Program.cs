using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace TestSenSim
{
    class Program
    {
        static void Main(string[] args)
        {
            processPGPI();
            //simpleTest();
            //speedTest();
        }

        static void processPGPI()
        {
            List<string> targets = new List<string>();
            List<string> pg = new List<string>();
            List<string> pi = new List<string>();
            int charCounter = 0;

            Dictionary<string, string> productGroups = new Dictionary<string, string>();

            // read product group file
            using (StreamReader file = new StreamReader("PG Only.csv"))
            {
                string line;
                for (int i = 0; i < 1; i++)
                    file.ReadLine(); // get past column header
                while ((line = file.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    
                    string desc = columns[1].TrimEnd(' ');
                    desc = desc.Replace('&', ' ');
                    desc = desc.Replace('\'', ' ');
                    desc = desc.Replace('\"', ' ');
                    desc = desc.Replace(';', ' ');
                    desc = desc.Replace('+', ' ');

                    if(!productGroups.ContainsKey(columns[0]))
                        productGroups.Add(columns[0], desc);
                }
            }

            using (StreamReader file = new StreamReader("Active PGPI.csv"))
            {
                string line;
                for(int i = 0; i < 1; i++)
                    file.ReadLine(); // get past column header
                while ((line = file.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    pg.Add(columns[0]);
                    pi.Add(columns[1]);
                    string desc = columns[2].TrimEnd(' ');
                    desc = desc.Replace('&', ' ');
                    desc = desc.Replace('\'', ' ');
                    desc = desc.Replace('\"', ' ');
                    desc = desc.Replace(';', ' ');
                    desc = desc.Replace('+', ' ');
                    string productDesc = "";
                    if (productGroups.ContainsKey(columns[0]))
                    {
                        productDesc = productGroups[columns[0]];
                        desc = desc + " " + productDesc;
                    }
                    targets.Add(desc);
                    charCounter += desc.Length + 1;
                    if(charCounter > 2000)
                    {
                        // get the embeddings
                        List<double[]> embeddings = SenSim.SemanticSimilarity.getEmbeddings(targets);

                        // update new pgpi file
                        for (int i = 0; i < targets.Count; i++)
                        {
                            File.AppendAllText("pgpiEmbeddingsExt.csv", pg[i] + "," + pi[i] + "," + targets[i] + "," + string.Join(",", embeddings[i]) + Environment.NewLine);
                        }

                        // reset
                        targets = new List<string>();
                        pg = new List<string>();
                        pi = new List<string>();
                        charCounter = 0;
                    }
                }
            }
        }

        static void simpleTest()
        {
            string[] targets = { "red fox", "hi there earth", "a buffalo can leap atop the podium" };
            string[] candidates = { "blue dog", "white castle", "this is a full sentence", "the quick brown fox jumps over the lazy dog", "hello world" };

            List<Dictionary<string, double>> scoresFull = SenSim.SemanticSimilarity.CalcSimilarity(targets, candidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine);

            Console.WriteLine(ToDebugString<string, double>(scoresFull[0]));
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static string ToDebugString<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
        {
            return "{" + string.Join(",", dictionary.Select(kv => kv.Key + "=" + kv.Value).ToArray()) + "}";
        }

        static void speedTest()
        {
            string target = "red fox";
            string[] targets = { "red fox", "hi there earth", "a buffalo can leap atop the podium" };
            string[] candidates = { "blue dog", "white castle", "this is a full sentence", "the quick brown fox jumps over the lazy dog", "hello world" };

            // call once ahead of time in case the service needs to load the models
            Dictionary<string, double> prescores = SenSim.SemanticSimilarity.CalcSimilarity(target, candidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine);
            List<Dictionary<string, double>> prescoresFull = SenSim.SemanticSimilarity.CalcSimilarity(targets, candidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine);

            Stopwatch swSingle = new Stopwatch();
            Stopwatch swMulti = new Stopwatch();

            swSingle.Start();
            Dictionary<string, double> scores = SenSim.SemanticSimilarity.CalcSimilarity(target, candidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine);
            swSingle.Stop();

            swMulti.Start();
            List<Dictionary<string, double>> scoresFull = SenSim.SemanticSimilarity.CalcSimilarity(targets, candidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine);
            swMulti.Stop();

            Console.WriteLine(swSingle.ElapsedMilliseconds);
            Console.WriteLine(swMulti.ElapsedMilliseconds);

            while (true) ;
        }
    }
}
