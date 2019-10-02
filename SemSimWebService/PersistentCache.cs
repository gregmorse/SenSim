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

        private static Dictionary<string, Dictionary<string, double>> cache;

        public static void put(string key, Dictionary<string, double> value)
        {
            // place object into memory cache
            cache.Add(key, value);

            // update persistent cache
            //if (!File.Exists(cacheFile))
            //    File.Create(cacheFile, );
            
            string dictString = string.Join(";", value);            
            File.AppendAllText(cacheFile, key + ":" + dictString + Environment.NewLine);
        }

        public static Dictionary<string, double> get(string key)
        {
            // if cache is empty, check cachefile and load into memory cache
            if(cache == null)
            {
                cache = new Dictionary<string, Dictionary<string, double>>();

                if(File.Exists(cacheFile))
                {
                    string line;
                    using (StreamReader file = new StreamReader(cacheFile))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            string[] parts = line.Split(':');
                            Dictionary<string, double> dict = new Dictionary<string, double>();

                            string[] entries = parts[1].Split(';');
                            for(int i = 0; i < entries.Length; i++)
                            {
                                string[] components = entries[i].Split(',');
                                string compKey = components[0].Remove(0, 1);
                                string compValueStr = components[1].Remove(0, 1);
                                compValueStr = compValueStr.Remove(compValueStr.Length - 1);
                                double compValue = Double.Parse(compValueStr);
                                dict.Add(compKey, compValue);
                            }
                            cache.Add(parts[0], dict);
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