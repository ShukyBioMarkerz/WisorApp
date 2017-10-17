using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLibrary.Logic;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.DataObjects
{
    class HistoricRateBBBR /*: HistoricRate*/
    {
        // make the class singltone
        static HistoricRateBBBR instanceBBBR;
        DateValueCollection historicDataBBBR;

        public static HistoricRateBBBR InstanceBBBR
        {
            get
            {
                if (null == instanceBBBR)
                {
                    instanceBBBR = new HistoricRateBBBR(MiscUtilities.GetHistoricRatesBBBRFilename());
                }
                return instanceBBBR;
            }
        }

        HistoricRateBBBR(string fn) 
        {
            historicDataBBBR = MiscUtilities.LoadHistoricRateFile(fn);
        }

        public static double GetHistoricIndex(indices indic, DateTime dateLoanTaken)
        {
            double index = 0;

            if (null == instanceBBBR)
            {
                instanceBBBR = new HistoricRateBBBR(MiscUtilities.GetHistoricRatesBBBRFilename());
            }

            if (null != instanceBBBR.historicDataBBBR)
            {
                DateValue dv = instanceBBBR.historicDataBBBR.GetNearest(dateLoanTaken);
                index = dv.Value;
            }
            return index;
        }

    }
}
