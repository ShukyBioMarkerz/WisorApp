using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WisorLib.MiscConstants;

namespace WisorLib
{
    public class Share
    {
        public static FieldList theCriteriaFields { get; set; }

        public static string theCriteriaFilename { get; set; }


        public static ProductsList theProducts { get; set; }

        public static string theProductsFilename { get; set; }


        public static SelectionType theSelectionType { get; set; }
}
}
