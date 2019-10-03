using SemSimWebService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SemSimWebService.Helpers
{
    public static class SenSimHelper
    {
        private static int RESULTCOUNT = 5;
        private static PGPI pgpi;

        public static IEnumerable<PGPIResponse> GetPGPIResponses(string id)
        {
            Tuple<string, string, double>[] cacheValue = PersistentCache.get(id);

            if (cacheValue != null)
            {
                return ConvertTupleToList(cacheValue);
            }

            if (pgpi == null)
                pgpi = new PGPI();

            string[] allPGPI = pgpi.getAllPGPIShortDesc();

            List<KeyValuePair<string, double>> topN = getTopN(SenSim.SemanticSimilarity.CalcSimilarity(id, pgpi.getEmbeddings(), SenSim.DistanceMetric.Cosine), RESULTCOUNT).ToList();
            cacheValue = new Tuple<string, string, double>[topN.Count];
            for (int i = 0; i < cacheValue.Length; i++)
            {
                string prod = pgpi.getPGPIShortDesc(topN[i].Key);
                cacheValue[i] = new Tuple<string, string, double>(prod, topN[i].Key, topN[i].Value);
            }
            PersistentCache.put(id, cacheValue);

            return ConvertTupleToList(cacheValue);
        }

        private static Dictionary<string, double> getTopN(Dictionary<string, double> allResults, int n)
        {
            var resultsList = allResults.ToList();
            resultsList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            Dictionary<string, double> ret = new Dictionary<string, double>();
            for (int i = 0; i < n; i++)
                if (i < resultsList.Count)
                    ret.Add(resultsList[i].Key, resultsList[i].Value);

            return ret;
        }

        private static List<PGPIResponse> ConvertTupleToList(Tuple<string, string, double>[] _toConvert)
        {
            List<PGPIResponse> ret = new List<PGPIResponse>();

            _toConvert.ToList().ForEach(s =>
            {
                ret.Add(new PGPIResponse
                {
                    Description = s.Item2,
                    TransactionTypeCode = s.Item1,
                    PercentMatch = s.Item3
                });
            });

            return ret;
        }
    }
}