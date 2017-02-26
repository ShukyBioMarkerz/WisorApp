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
            try
            {
                for (int i = 0; i < val.Length - startingIndex; i++)
                {
                    if (!String.IsNullOrEmpty(val[startingIndex + i]))
                        value[i] = Convert.ToDouble(val[startingIndex + i]);
                    else
                        value[i] = MiscConstants.UNDEFINED_DOUBLE;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: RateLine got Exception: " + e.ToString());
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
        string productID;
        int profile;

        public RatesKey(string id, int p)
        {
            productID = id;
            profile = p;
        }

        public override string ToString()
        {
            return "productID: " + productID + ", profile: " + profile;
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
            return (this.productID + this.profile).GetHashCode();
        }
    }



    /// <summary>
    /// ///////////////////////
    /// </summary>

}
