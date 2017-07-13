using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WisorLib;
using WisorLibrary.DataObjects;
using WisorLibrary.Testings;
using WisorLibrary.Utilities;
using static WisorLib.GenericProduct;
using static WisorLib.MiscConstants;

namespace WisorAppWpf 
{
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        


        public MainWindow()
        {
            InitializeComponent();

            // cleanup
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(on_processExit);

            // the logger window          
            DataContext = LogEntries = new ObservableCollection<LogEntry>();

            SetLogger(Log2Window);

            InitSettings();

            LoadMarket.Visibility = Visibility.Hidden;
        }

        static void on_processExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting.....");
            MiscUtilities.CleanUp();
        }

        private void InitSettings()
        {

            //SetRunLoanFunc(Utilities.RunTheLoansSync);
            SetRunLoanFunc(MultiThreadingManagment.RunTheLoanASync);
            SetRunLoanFuncSync(MultiThreadingManagment.RunTheLoansWraperSync);
            SetRunLoanFuncASync(MultiThreadingManagment.RunTheLoansWraperASync);

            Share.shouldShowCriteriaSelectionWindow = false;
            Share.shouldShowCriteriaSelectionContinue = false;
            Share.shouldShowProductSelectionWindow = false;
            Share.shouldShowProductSelectionContinue = false;
            Share.shouldShowRatesSelectionWindow = false;
            Share.shouldShowLoansSelectionWindow = false;
            
            Share.shouldRunSync = true;
            Share.shouldRunLogicSync = true;

            Share.shouldRunFake = false;

            Share.numberOfOption = 3;
 
            Share.shouldPrintCounters = false;
            Share.CalculatePmtCounter = Share.CalculateLuahSilukinCounter = Share.RateCounter =
                Share.counterOfOneDivisionOfAmounts =  Share.CalculatePmtFromCalculateLuahSilukinCounter =
                Share.OptionObjectCounter = Share.SavedCompositionsCounter =
                Share.CalculateLuahSilukinCounterNOTInFirstTimePeriod =
                Share.CalculateLuahSilukinCounterInFirstTimePeriod =
                Share.CalculateLuahSilukinCounterIndexUsedFirstTimePeriod = 0;

            //Share.ShouldCalcTheBankProfit = true;
            Share.numberOfPrintResultsInList = 1; //  100;

            Share.ShouldEachCombinationRunSeparetly = false;
            Share.ShouldStoreAllCombinations = false;

            Share.shouldCreateHTMLReport = true;
            Share.shouldCreatePDFReport = true;
            Share.ShouldStoreInDB = true;
            Share.ShouldStoreHTMLReport = true;
            Share.LoansLoadFromLine = MiscConstants.UNDEFINED_UINT;
            Share.LoansLoadIDsFromLine = MiscConstants.UNDEFINED_STRING;
            Share.shouldDebugLoans = false;
            Share.shouldDebugLuchSilukin = false;


            // load the configuration file
            MiscUtilities.LoadXMLConfigurationFile(MiscConstants.CONFIGURATION_FILE);

            // output log level settings
            Share.printMainInConsole = true;
            Share.printToOutputFile = true;
            Share.printFunctionsInConsole = false;
            Share.printSubFunctionsInConsole = false;
            Share.printPercentageDone = false;
            Share.NumberOfCanRefininceLoans = 0;
            Share.NumberOfPositiveBeneficialLoans = 0;


            // testing area
            //Tests.TestLuchSilukinCalculation();
            //Tests.TestTimeCulture();
            //Tests.TestCompositionLogic();
            //Tests.TestSizeSortedList();
            //Tests.TestPDFCreation();
            //Tests.TestReportCreation();
            //Tests.TestRegularExpression();
            //Tests.TestXMLFunctionality("123");
            //Tests.TestHistoricIndexRate();
            //Tests.TestRatesLoading();
            //Tests.TestCombinations();
            //Tests.SendSimpleMessage();

        }

        private void SetLogger(MyDelegate func)
        {
            WindowsUtilities.loggerMethod = func;
        }

        private void SetRunLoanFunc(MyRunDelegate func)
        {
            WindowsUtilities.runLoanMethod = func;
        }

        private void SetRunLoanFuncSync(MyRunDelegateListOfLoans func)
        {
            WindowsUtilities.runLoanMethodSync = func;
        }
        private void SetRunLoanFuncASync(MyRunDelegateListOfLoans func)
        {
            WindowsUtilities.runLoanMethodASync = func;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        private void AddEntry(string msg)
        {
            LogEntry le = new LogEntry()
            {
                //Index = index++,
                DateTime = DateTime.Now,
                Message = msg
            };

            Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(le)));
        }


        public void Log2Window(string msg, bool write2console = true, bool shouldColor = false)
        {
            AddEntry(msg);
            if (write2console)
            {
                if (shouldColor)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }
                Console.WriteLine(msg);
                if (shouldColor)
                {
                    Console.ResetColor();
                }
            }
        }

         private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Share.numberOfPrintResultsInList = 0;
            SetButtonEnable(false);
            Utilities.RunTheLogic();
            //// tbd shuky
            //MiscUtilities.CloseSummaryFile();

        }


        private void AskInput_Button_Click(object sender, RoutedEventArgs e)
        {
            SetButtonEnable(false);
            Utilities.Ask4Input();
            // tbd shuky
            //MiscUtilities.CloseSummaryFile();
        }

        
        private void SelectMarket_Load(object sender, RoutedEventArgs e)
        {
            // ... A List.
            List<string> data = new List<string>();
            var names = Enum.GetNames(typeof(markets));

            foreach (string m in names)
            {
                data.Add(m);
                //LoadMarket.Items.Add(m);
            }

            // ... Assign the ItemsSource to the List.
            LoadMarket.ItemsSource = data;
            //LoadMarket.Text = "Select the market:";

            // ... Make the first item selected.
            //LoadMarket.SelectedIndex = 0;
        }

        private void SelectMarket_selected(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as ComboBox;

            // ... Set SelectedItem as Window Title.
            string value = comboBox.SelectedItem as string;
            RunEnvironment.SetMarket((markets)Enum.Parse(typeof(markets), value, true));
        }
        
    
        

  
        private void SetButtonEnable(bool val)
        {
            StartButton.IsEnabled = val;
            AskInput.IsEnabled = val;
            AskInput.Visibility = Visibility.Hidden;
        }

   
        // Logger data
        //private string TestData = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";
        //private List<string> words;
        //private int maxword;
        //private int index;

        public ObservableCollection<LogEntry> LogEntries { get; set; }


        
   
    }


    ////////

    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        //public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }


    //////////////////////////

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }

}
