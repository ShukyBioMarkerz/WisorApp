using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;

namespace WisorLib
{

    /// <summary>
    /// ////////////////////////////
    /// 


    public class FieldList : List<CriteriaField>
    {
        public static Predicate<CriteriaField> CriteriaFieldPredicate(CriteriaField cf)
        {
            return delegate (CriteriaField field)
            {
                return field.ID.ToLower() == cf.ID.ToLower();
            };
        }

        public int GetIndexOf(string fieldName)
        {
            int index = -1;
            CriteriaField cf = this.Find(CriteriaFieldPredicate(new CriteriaField(fieldName)));
            if (null != cf)
                index = cf.index;
            return index;
        }

        public CriteriaField GetField(string fieldName)
        {
           return this.Find(CriteriaFieldPredicate(new CriteriaField(fieldName)));
        }
    }



    public class CriteriaField
    {
        public string ID { get; set; }
        public int index { get; set; }
        public bool status { get; set; }
        public string value { get; set; }
        public bool legal { get; set; }
        public int min { get; set; }
        public int max { get; set; }
        public bool isMandatory { get; set; }
        public string type { get; set; }
        public List<string> options { get; set; }

        public string toString()
        {
            return ("CriteriaField ID:" + ID + " status: " + status + " value: " + value + " legal: " + legal);
        }

        // constructor
        public CriteriaField(string id = "", int index = MiscConstants.UNDEFINED_INT, string value = "",
            int min = MiscConstants.UNDEFINED_INT, int max = MiscConstants.UNDEFINED_INT, bool isMandatory = false,
            string type = "", List<string> options = null)
        {
            this.ID = id;
            this.index = index;
            this.value = value;
            this.legal = true;
            this.status = true;
            this.min = min;
            this.max = max;
            this.isMandatory = isMandatory;
            this.type = type;
            this.options = options;
        }

    }

    //// Define the mandatory cretiria which shoild be defined while using the calculation
    //public class MandatoryCriteriaFieldList : FieldList
    //{
    //    public MandatoryCriteriaFieldList() {
    //        int index = 1;
    //        Add(new CriteriaField(MiscConstants.LOAN_AMOUNT, index++));
    //        Add(new CriteriaField(MiscConstants.MONTHLY_PAYMENT, index++));
    //        Add(new CriteriaField(MiscConstants.PROPERTY_VALUE, index++));
    //        Add(new CriteriaField(MiscConstants.YEARLY_INCOME, index++));
    //        Add(new CriteriaField(MiscConstants.AGE, index++));
    //    }


    //}



}
