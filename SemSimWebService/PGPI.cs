using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace SemSimWebService
{
    public class PGPI
    {
        private static string pgpiFile = HttpContext.Current.Server.MapPath(@"..\..\..\pgpiEmbeddings.csv");

        Dictionary<string, string> shortDesc, longDesc;
        Dictionary<string, double[]> embeddings;

        public PGPI()
        {
            shortDesc = new Dictionary<string, string>();
            longDesc = new Dictionary<string, string>();
            embeddings = new Dictionary<string, double[]>();

            using (StreamReader file = new StreamReader(pgpiFile))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] columns = line.Split(',');
                    string pgpi = columns[0] + columns[1];
                    string desc = columns[2];
                    double[] embedding = new double[512];
                    for (int i = 0; i < 512; i++)
                        embedding[i] = Double.Parse(columns[i + 3]);
                    desc = desc.TrimEnd(' ');

                    if(!shortDesc.ContainsKey(desc))
                        shortDesc.Add(desc, pgpi);
                    if (!longDesc.ContainsKey(desc))
                        longDesc.Add(desc, pgpi);
                    if (!embeddings.ContainsKey(desc))
                        embeddings.Add(desc, embedding);
                }
            }
        }

        public Dictionary<string, double[]> getEmbeddings()
        {
            return embeddings;
        }

        public string[] getAllPGPIShortDesc()
        {            
            return shortDesc.Keys.ToArray();
        }

        public string[] getAllPGPILongDesc()
        {
            return longDesc.Keys.ToArray();
        }

        public string getPGPIShortDesc(string desc)
        {
            return shortDesc[desc];
        }

        public string getPGPILongDesc(string desc)
        {
            return longDesc[desc];
        }
    }
}