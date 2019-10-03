using SemSimWebService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SemSimWebService.Controllers
{
    public class CalcScoreController : ApiController
    {
        private static int RESULTCOUNT = 5;
        private PGPI pgpi;

        [Route("api/CalcScore/short/{id}")]
        public IEnumerable<PGPIResponse> Get(string id)
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
            for(int i = 0; i < cacheValue.Length; i++)
            {
                string prod = pgpi.getPGPIShortDesc(topN[i].Key);
                cacheValue[i] = new Tuple<string, string, double>(prod, topN[i].Key, topN[i].Value);
            }
            PersistentCache.put(id, cacheValue);

            return ConvertTupleToList(cacheValue);
        }
        
        private List<PGPIResponse> ConvertTupleToList(Tuple<string, string, double>[] _toConvert)
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

        [Route("api/CalcScore/{id}/{candidates}")]
        public Dictionary<string, double> Get(string id, string candidates)
        {
            Dictionary<string, double> ret;

            string[] candidateValues = candidates.Split(',');

            ret = getTopN(SenSim.SemanticSimilarity.CalcSimilarity(id, candidateValues, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine), RESULTCOUNT);

            return ret;
        }

        private Dictionary<string, double> getTopN(Dictionary<string, double> allResults, int n)
        {
            var resultsList = allResults.ToList();
            resultsList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            Dictionary<string, double> ret = new Dictionary<string, double>();
            for (int i = 0; i < n; i++)
                if(i < resultsList.Count)
                    ret.Add(resultsList[i].Key, resultsList[i].Value);

            return ret;
        }        

        // POST: api/CalcScore
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/CalcScore/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/CalcScore/5
        public void Delete(int id)
        {
        }
    }
}
