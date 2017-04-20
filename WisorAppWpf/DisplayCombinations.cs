using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WisorLib;
using WisorLibrary.Utilities;

namespace WisorAppWpf
{
    public partial class DisplayCombinations : Form
    {
        public DisplayCombinations()
        {
            InitializeComponent();
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    List<ChosenComposition> comp = orderByBorrower(compositions);
        //}

        private List<ChosenComposition> orderByBorrower(List<ChosenComposition> compositions)
        {
            return MiscUtilities.OrderCompositionListByBorrower(compositions);
        }

        private List<ChosenComposition> orderByBank(List<ChosenComposition> compositions)
        {
            return MiscUtilities.OrderCompositionListByBank(compositions);
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    List<ChosenComposition> comp = orderByBank(compositions);
        //}

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
