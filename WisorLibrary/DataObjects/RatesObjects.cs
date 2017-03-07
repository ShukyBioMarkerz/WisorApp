using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLibrary.DataObjects
{
    /// <summary>
    /// ///////////////////////
    /// </summary>
    public class RateLine
    {
        double[] value;

        // [] operator
        public double this[int index]
        {
            get
            {
                return value[index];
            }
        }

        public int Length()
        {
            return value.Length;
        }

        public RateLine(string[] val, int startingIndex)
        {
            value = new double[val.Length];
            string curr = MiscConstants.UNDEFINED_STRING;

            try
            {
                for (int i = 0; i < val.Length - startingIndex; i++)
                {
                    curr = val[startingIndex + i];
                    if (!String.IsNullOrEmpty(val[startingIndex + i]))
                        value[i] = Convert.ToDouble(val[startingIndex + i]);
                    else
                        value[i] = MiscConstants.UNDEFINED_DOUBLE;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: RateLine got Exception: " + e.ToString() + ". Current: " + curr);
            }
        }

        public override string ToString()
        {
            string str = "Values: ";
            for (int i = 0; i < value.Length; i++)
            {
                str += value[i] + ",";
            }
            return str;
        }
    }

    /// <summary>
    /// ///////////////////////
    /// </summary>
    /// 

    public class RatesKey
    {
        public int productID;
        public int profile;

        public RatesKey(int id, int p)
        {
            productID = id;
            profile = p;
        }

        public override string ToString()
        {
            return "productID: " + GenericProduct.GetProductName(productID).ToString() + ", profile: " + profile;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            RatesKey o = obj as RatesKey;
            if (o == null)
                return false;

            // Return true if the fields match:
            return (productID == o.productID) && (profile == o.profile);
        }

        public override int GetHashCode()
        {
            // TBD - Performance: should handle properly
            return (productID + 23 * profile).GetHashCode();
            // performance wise. try to ease the cpu time
            //int h = 0;
            //for (int i = 0; i < productID.Length; i++)
            //    h += productID[i] * profile ^ productID.Length - (i + 1);
            //return h;
        }
    }

}
