using System;
using System.Collections.Generic;
using System.IO;
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


        static public string displayFileDialog(string header)
        {
            string filename = null;
            // Displays an OpenFileDialog so the user can select a file
            /*Microsoft.Win32.*/OpenFileDialog openFileDialog1 = new /*Microsoft.Win32.*/OpenFileDialog();
            openFileDialog1.Filter = "all files (*.*)|*.*|XML files (*.xml)|*.xml";
            // openFileDialog1.FileName = nfn.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1 /*Path.DirectorySeparatorChar*/);
            openFileDialog1.Title = header;

            // Show the Dialog.
            if (DialogResult.OK ==  openFileDialog1.ShowDialog())
            {
                /*nfn = */filename = openFileDialog1.FileName; //    string fn = @"../../Gui.xml";
            }
            return filename;
        }
    
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
