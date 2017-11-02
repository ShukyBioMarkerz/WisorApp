using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.DataObjects
{
    public class HistoricRate
    {
        // make the class singltone
        private static HistoricRate instance;
        public string filename { get; set; }
        public bool Status { get; internal set; }
        DateValueCollection historicData
        {
            get; set;
        }

        public static HistoricRate Instance
        {
            get
            {
                if (markets.ISRAEL == Share.theMarket)
                {
                    if (null == instance)
                    {
                        instance = new HistoricRate(MiscUtilities.GetHistoricRatesFilename());
                    }
                    return instance;
                }
                else
                    return null;
            }
        }

        HistoricRate(string fn)
        {
            historicData = MiscUtilities.LoadHistoricRateFile(fn);
        }

        public static double GetHistoricIndex(indices indic, DateTime dateLoanTaken)
        {
            double index = 0;

            if (null == instance || null == Instance.historicData)
            {
                instance = new HistoricRate(MiscUtilities.GetHistoricRatesFilename());
            }

            if (null != Instance.historicData)
            {
                DateValue dv = Instance.historicData.GetNearest(dateLoanTaken);
                index = dv.Value;
                
            }
            return index;
        }

        public static double GetHistoricValues(indices indic, DateTime fromDate, DateTime toDate)
        {
            double index = 0;
            // Instance.historicData.FindAll(Predicate < DateTime > pred);
            if (null != Instance.historicData)
            {
                index = Instance.historicData.GetValueFromTo(fromDate, toDate);
            }
            return index;
        }
        
    }


}
