using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace SemSimWebService
{
    public class PersistentCache
    {
        private static string cacheFile = HttpContext.Current.Server.MapPath(@"..\..\..\cacheFile.txt");

        private static Dictionary<string, Tuple<string, string, double>[]> cache;

        public static void put(string key, Tuple<string, string, double>[] value)
        {
            // place object into memory cache
            cache.Add(key, value);

            // update persistent cache
            //if (!File.Exists(cacheFile))
            //    File.Create(cacheFile, );
            
            string valueString = "";      
            for(int i = 0; i < value.Length; i++)
            {
                valueString += value[i].Item1 + ',' + value[i].Item2 + ',' + value[i].Item3;
                if (i < value.Length - 1)
                    valueString += ';';
            }
            File.AppendAllText(cacheFile, key + ":" + valueString + Environment.NewLine);
        }

        public static Tuple<string, string, double>[] get(string key)
        {
            // if cache is empty, check cachefile and load into memory cache
            if(cache == null)
            {
                cache = new Dictionary<string, Tuple<string, string, double>[]>();

                if(File.Exists(cacheFile))
                {
                    string line;
                    using (StreamReader file = new StreamReader(cacheFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] parts = line.Split(':');
                            List<Tuple<string, string, double>> values = new List<Tuple<string, string, double>>();

                            string[] entries = parts[1].Split(';');
                            for(int i = 0; i < entries.Length; i++)
                            {
                                string[] components = entries[i].Split(',');
                                string item1 = components[0];
                                string item2 = components[1];
                                string compValueStr = components[2];                                
                                double compValue = Double.Parse(compValueStr);
                                values.Add(new Tuple<string, string, double>(item1, item2, compValue));
                            }
                            cache.Add(parts[0], values.ToArray());
                        }
                    }
                }
            }

            if (cache.ContainsKey(key))
                return cache[key];
            else
                return null;
        }
    }
}