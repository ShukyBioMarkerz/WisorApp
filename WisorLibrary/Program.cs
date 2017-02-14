using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WisorLib
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            // Start: convert a .NET console application to a Winforms or WPF application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            // End: convert a .NET console application to a Winforms or WPF application

            /*
            Console.Write("Enter loan to value ratio (LTV): ");
            CalculationParameters.ltv = double.Parse(Console.ReadLine());
            Console.Write("Enter payment to income ratio (PTI): ");
            CalculationParameters.pti = double.Parse(Console.ReadLine());
            BorrowerProfile bp = new BorrowerProfile();
            */

            //Rates.ReadInterestRateFileAndUpdateRatesInSoftware(3);


            /*
            double[][] ratesOutput = new double[12][];
            for (int counter = 0; counter < 12; counter++)
            {
                ratesOutput[counter] = new double[27];
            }
            
            System.IO.StreamReader fileReader = new System.IO.StreamReader("../../../RateFile.csv");
            string firstLineInput = fileReader.ReadLine();
            for (int i = 0; i < 12; i ++)
            {
                string lineInput = fileReader.ReadLine();
                string[] lineValues = lineInput.Split(',');
                Console.WriteLine("Borrower Profile : " + lineValues[1]);
                for (int j = 2; j < lineValues.Length; j++)
                {
                    ratesOutput[i][j - 2] = double.Parse(lineValues[j]);
                    Console.WriteLine(ratesOutput[i][j - 2]);
                }
                Console.WriteLine();
            }
            */


            //// Shuky - moved this to the UI activity
            //FastSearch fastSearch = new FastSearch();

            //Console.WriteLine("\nPress any key to close software.");
            //Console.ReadKey();
            

        }
    }
}
