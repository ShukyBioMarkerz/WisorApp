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
            rates = LoadRatesFile(filename);
            WindowsUtilities.loggerMethod("NOTICE LoadRates succesfully load: " + rates.Count + " entries.");
            return (null != rates);
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

            if (!rates.TryGetValue(key, out rate))
                rate = null;
            if (null != rate)
                if (index <= rate.Length())
                result = rate[index];
            return result;
        }


        private Dictionary<RatesKey, RateLine> LoadRatesFile(string filename)
        {
            Dictionary<RatesKey, RateLine> dic = new Dictionary<RatesKey, RateLine>();
  
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
                        if (String.IsNullOrEmpty(line))
                            continue;

                        // skip the first line
                        // TBD: should read the headers and relate to it
                        if (1 == lineNumber++)
                            continue;

                        string[] entities = line.Split(',');

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
                Console.WriteLine("ERROR: LoadRatesFile got Exception: " + e.ToString());
            }

            return dic;
        }

    }
}
