using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestSenSim
{
    class Program
    {
        static void Main(string[] args)
        {
            simpleTest();
            //speedTest();
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
