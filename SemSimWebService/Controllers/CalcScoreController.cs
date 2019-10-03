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
        public Tuple<string, string, double>[] Get(string id)
        {
            Tuple<string, string, double>[] ret = PersistentCache.get(id);
            if (ret != null)
                return ret;

            if (pgpi == null)
                pgpi = new PGPI();

            string[] allPGPI = pgpi.getAllPGPIShortDesc();            

            List<KeyValuePair<string, double>> topN = getTopN(SenSim.SemanticSimilarity.CalcSimilarity(id, pgpi.getEmbeddings(), SenSim.DistanceMetric.Cosine), RESULTCOUNT).ToList();
            ret = new Tuple<string, string, double>[topN.Count];
            for(int i = 0; i < ret.Length; i++)
            {
                string prod = pgpi.getPGPIShortDesc(topN[i].Key);
                ret[i] = new Tuple<string, string, double>(prod, topN[i].Key, topN[i].Value);
            }
            PersistentCache.put(id, ret);
            return ret;
        }
        /*
        public Dictionary<string, double> Get(string id)
        {            
            Dictionary<string, double> ret = PersistentCache.get(id);
            if (ret != null)
                return ret;

            string[] allinput = id.Split(',');
            string[] allCandidates = new string[allinput.Length - 1];
            for (int i = 0; i < allCandidates.Length; i++)
                allCandidates[i] = allinput[i + 1];
            ret = getTopN(SenSim.SemanticSimilarity.CalcSimilarity(allinput[0], allCandidates, SenSim.SimilarityModel.USETrans, SenSim.DistanceMetric.Cosine), RESULTCOUNT);
            PersistentCache.put(id, ret);
            return ret;
        }
        */

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
