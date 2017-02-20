using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WisorLib
{

    public delegate void MyDelegate(string s);
   

    public class WindowsUtilities
    {
        static private MyDelegate method;

        public static void SetLogger(MyDelegate func)
        {
            method = func;
        }

        public static MyDelegate GetLogger()
        {
            return method;
        }

    }
}
