using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SenSim
{
    public enum SimilarityModel
    {
        Spacy,
        USETrans,
        USEDAN
    }

    public enum DistanceMetric
    {
        Euclidean,
        Cosine
    }

    public class SemanticSimilarity
    {
        private static string baseURL = "http://localhost:8080"; //ConfigurationManager.AppSettings["baseURL"];

        public static Dictionary<string, double> CalcSimilarity(string target, string[] candidates, SimilarityModel model = SimilarityModel.Spacy, DistanceMetric metric = DistanceMetric.Euclidean)
        {
            Dictionary<string, double> results = new Dictionary<string, double>();
            string uri;

            switch(model)
            {
                case SimilarityModel.Spacy:
                    for(int i = 0; i < candidates.Length; i++)
                    {
                        uri = baseURL + @"/callspacy?string1=""" + target + "\"&string2=\"" + candidates[i] + "\"";
                        results.Add(candidates[i], Double.Parse(GetRequest(uri)));                        
                    }
                    break;                    

                case SimilarityModel.USEDAN:
                    uri = baseURL + @"/callUSEDAN?data=""" + target;
                    for(int i = 0; i < candidates.Length; i++)
                    {
                        uri += "," + candidates[i];
                    }
                    uri += "\"";
                    List<double> scores = CalcScore(GetRequest(uri), 0, 1, metric);
                    for (int i = 0; i < scores.Count; i++)
                        results.Add(candidates[i], scores[i]);
                    break;

                case SimilarityModel.USETrans:
                    uri = baseURL + @"/callUSETrans?data=""" + target;
                    for (int i = 0; i < candidates.Length; i++)
                    {
                        uri += "," + candidates[i];
                    }
                    uri += "\"";
                    scores = CalcScore(GetRequest(uri), 0, 1, metric);
                    for (int i = 0; i < scores.Count; i++)
                        results.Add(candidates[i], scores[i]);
                    break;
            }

            return results;
        }

        public static List<Dictionary<string, double>> CalcSimilarity(string[] targets, string[] candidates, SimilarityModel model = SimilarityModel.Spacy, DistanceMetric metric = DistanceMetric.Euclidean)
        {
            List<Dictionary<string, double>> results = new List<Dictionary<string, double>>();

            string uri;

            switch (model)
            {
                case SimilarityModel.Spacy:
                    for (int h = 0; h < targets.Length; h++)
                    {
                        results[h] = new Dictionary<string, double>();
                        for (int i = 0; i < candidates.Length; i++)
                        {
                            uri = baseURL + @"/callspacy?string1=""" + targets[h] + "\"&string2=\"" + candidates[i] + "\"";
                            results[h].Add(candidates[i], Double.Parse(GetRequest(uri)));
                        }
                    }
                    break;

                case SimilarityModel.USEDAN:
                    uri = baseURL + @"/callUSEDAN?data=""" + targets[0];
                    for(int i = 1; i < targets.Length; i++)
                    {
                        uri += "," + targets[i];
                    }
                    for (int i = 0; i < candidates.Length; i++)
                    {
                        uri += "," + candidates[i];
                    }
                    uri += "\"";

                    string requestResult = GetRequest(uri);

                    for (int i = 0; i < targets.Length; i++)
                    {
                        List<double> scores = CalcScore(requestResult, i, targets.Length, metric);
                        for (int j = 0; j < scores.Count; j++)
                            results[i].Add(candidates[j], scores[j]);
                    }
                    
                    break;

                case SimilarityModel.USETrans:
                    uri = baseURL + @"/callUSETrans?data=""" + targets[0];
                    for (int i = 1; i < targets.Length; i++)
                    {
                        uri += "," + targets[i];
                    }
                    for (int i = 0; i < candidates.Length; i++)
                    {
                        uri += "," + candidates[i];
                    }
                    uri += "\"";

                    requestResult = GetRequest(uri);

                    for (int i = 0; i < targets.Length; i++)
                    {
                        List<double> scores = CalcScore(requestResult, i, targets.Length, metric);
                        results.Add(new Dictionary<string, double>());
                        for (int j = 0; j < scores.Count; j++)
                            results[i].Add(candidates[j], scores[j]);
                    }

                    break;
            }

            return results;
        }

        private static string GetRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static List<double> CalcScore(string fullText, int targetIndex, int candidateStartIndex, DistanceMetric metric)
        {
            List<double> result = new List<double>();

            // parse the string to get a list of embeddings
            List<double[]> embeddingValues = new List<double[]>();
            string[] embeddings = fullText.Split('[');
            for(int i = 2; i < embeddings.Length; i++) // indexes 0 and 1 will be blank
            {
                embeddings[i] = embeddings[i].Replace("]", "");
                embeddings[i] = embeddings[i].Replace("\n", "");
                string[] values = embeddings[i].Split(' ');
                List<double> valuesD = new List<double>();
                for (int j = 0; j < values.Length; j++)
                {
                    if(values[j] != "")
                        valuesD.Add(Double.Parse(values[j]));
                }

                embeddingValues.Add(valuesD.ToArray());
            }

            // calculate scores to target for each candidate embedding
            for(int i = candidateStartIndex; i < embeddingValues.Count; i++)
            {
                double score = 0;

                // Euclidean distance
                switch(metric)
                {
                    case DistanceMetric.Euclidean:
                        for (int j = 0; j < embeddingValues[i].Length; j++)
                        {
                            score += (embeddingValues[i][j] - embeddingValues[targetIndex][j]) * (embeddingValues[i][j] - embeddingValues[targetIndex][j]);
                        }
                        score = Math.Sqrt(score);
                        result.Add(1 - score);
                        break;

                    case DistanceMetric.Cosine:
                        // Cosine similarity
                        double dotProduct = 0;
                        double targetMagnitude = 0;
                        double candidateMagnitude = 0;
                        for (int j = 0; j < embeddingValues[i].Length; j++)
                        {
                            dotProduct += embeddingValues[i][j] * embeddingValues[targetIndex][j];
                            targetMagnitude += embeddingValues[targetIndex][j] * embeddingValues[targetIndex][j];
                            candidateMagnitude += embeddingValues[i][j] * embeddingValues[i][j];
                        }

                        targetMagnitude = Math.Sqrt(targetMagnitude);
                        candidateMagnitude = Math.Sqrt(candidateMagnitude);
                        score = dotProduct / (targetMagnitude * candidateMagnitude);
                        result.Add(score);
                        break;
                } 
            }

            return result;
        }
    }    
}
