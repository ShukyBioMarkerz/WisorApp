using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WisorLib
{

    public delegate void MyDelegate(string s);

    public delegate RunLoanDetails MyRunDelegate(loanDetails loan);

  
    public class WindowsUtilities
    {
        static public MyDelegate loggerMethod { get; set; }

        static public MyRunDelegate runLoanMethod { get; set; }

    
        //public static void SetLogger(MyDelegate func)
        //{
        //    loggerMethod = func;
        //}

        //public static MyDelegate GetLogger()
        //{
        //    return loggerMethod;
        //}

    }

}
