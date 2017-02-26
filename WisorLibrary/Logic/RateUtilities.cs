using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.DataObjects;

namespace WisorLibrary.Logic
{
    class RateUtilities
    {

        // make the class singltone
        private static RateUtilities instance;

        public static RateUtilities SetFilename(string filename)
        {
            //get
            //{
            if (null == instance)
            {
                    instance = new RateUtilities(filename);
                }
                return instance;
            //}
        }

        public static RateUtilities Instance
        {
            get
            {
                if (null == instance)
                {
                instance = new RateUtilities();
            }
            return instance;
            }
        }

        Dictionary<RatesKey, RateLine> rates;
        string filename;

        private RateUtilities(string filename = "")
        {
            bool rc = false;

            if (!String.IsNullOrEmpty(filename))
            {
                setFilename(filename);
                rc = LoadRates();
            }
            else
                WindowsUtilities.loggerMethod("NOTICE RateUtilities without setting the rates file name");
        }

        private void setFilename(string fn)
        {
            filename = fn;
        }

        public bool LoadRates()
        {
            if (null == rates || 0 < rates.Count)
                rates = LoadRatesCSVFile(filename);
            WindowsUtilities.loggerMethod("NOTICE LoadRates succesfully load: " + rates.Count + " entries.");
            return (null != rates && 0 < rates.Count);
        }

        public RateLine FindRatesForKey(RatesKey key)
        {
            RateLine rate;

            if (! rates.TryGetValue(key, out rate))
                rate = null;
            return rate;
        }

        public double FindRateForKey(RatesKey key, int index)
        {
            RateLine rate;
            double result = MiscConstants.UNDEFINED_DOUBLE;

            if (0 < rates.Count)
            {
                if (!rates.TryGetValue(key, out rate))
                    rate = null;
                if (null != rate)
                    if (index <= rate.Length())
                        result = rate[index];
            }
            else
            {
                Console.WriteLine("ERROR: no rates where loaded");
                WindowsUtilities.loggerMethod("ERROR: no rates where loaded");
            }

            if (0 > result)
            {
                Console.WriteLine("ERROR: illegal rate: " + result);
                WindowsUtilities.loggerMethod("ERROR:  illegal rate: " + result);
                // TBD
                //result = 0.015;
            }


            return result;
        }


        private Dictionary<RatesKey, RateLine> LoadRatesCSVFile(string filename)
        {
            Dictionary<RatesKey, RateLine> dic = new Dictionary<RatesKey, RateLine>();
            string curr = MiscConstants.UNDEFINED_STRING;
            string[] entities;

            try
            {
                if (File.Exists(filename))
                {
                    var file = new FileInfo(filename);
                    StreamReader fileReader = new System.IO.StreamReader(filename);
                    string line;
                    int lineNumber = 1;
                    do
                    {
                        line = fileReader.ReadLine();
                        curr = line;
                        if (String.IsNullOrEmpty(line))
                            continue;

                        // skip the first line
                        // TBD: should read the headers and relate to it
                        if (1 == lineNumber++)
                            continue;

                        entities = line.Split(',');

                        if (String.IsNullOrEmpty(entities[0]))
                        {
                            continue;
                        }

                        // ensure the line correctness
                        // TBD: should define properly the index with no hard coding...
                        if (3 < entities.Length)
                        {
                            // the first column is the ProductID
                            // the second column is the user profile
                            RatesKey ratesKey = new RatesKey(entities[0], Convert.ToInt32(entities[1]));

                            dic.Add(ratesKey, new RateLine(entities, 2 /*startingIndex*/));
                        }
                    }
                    while (! System.String.IsNullOrEmpty(line));

                }
                else
                {
                    Console.WriteLine("ERROR: LoadRatesFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: LoadRatesFile got Exception: " + e.ToString() + ". Curr: " + curr);
            }

            return dic;
        }

    }
}
