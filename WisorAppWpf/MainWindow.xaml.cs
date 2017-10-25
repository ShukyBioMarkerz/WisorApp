using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
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

#if DETECT_EXIT
        ////////////////////////////////// 
        // manage exiting brutaly start
  
        static void on_processExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting.....");
            MiscUtilities.CleanUp();
        }

  
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        private static bool isclosing = false;

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            // Put your own handler here
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+C received!");
                    break;

                case CtrlTypes.CTRL_BREAK_EVENT:
                    isclosing = true;
                    Console.WriteLine("CTRL+BREAK received!");
                    break;

                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    Console.WriteLine("Program being closed!");
                    break;

                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    isclosing = true;
                    Console.WriteLine("User is logging off!");
                    break;

            }
            return true;
        }
#endif
        // manage exiting brutaly end
        ////////////////////////////////// 


        public MainWindow()
        {
            InitializeComponent();

#if DETECT_EXIT
            // cleanup
            Closing += on_processExit;
            Console.CancelKeyPress += new ConsoleCancelEventHandler(on_processExit);
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
#endif

            // the logger window          
            DataContext = LogEntries = new ObservableCollection<LogEntry>();

            MiscUtilities.SetLogger(Log2Window);

            InitSettings();

         
#if DETECT_EXIT
            while (!isclosing)
            {
                Thread.Sleep(1000);
            }
#endif
        }


        private void InitSettings()
        {
            bool rc = MiscUtilities.PrepareRunningFull();
            // bool rc = MiscUtilities.SetupAllEnv();

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
            //Tests.TestHistoricIndexRateFromDB();
            //Tests.TestRatesLoading();
            //Tests.TestCombinations();
            // Tests.SendSimpleEmailMessage();
            //Tests.SendTestSimpleEmailMessageByMailgun();
            //Tests.TestRiskLiquidity();
            // Tests.TestCalculateMonthlyPmt();
            //Tests.TestlanguageFunctionality();
            // Tests.TestLongReportCreation();
            // Tests.TestDatedCalculation();
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
            // avoid the smart logger window...
            // Console.WriteLine(msg);
            return;

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
