﻿using Microsoft.Win32;
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

            // the logger window          
            DataContext = LogEntries = new ObservableCollection<LogEntry>();

            SetLogger(Log2Window);

            InitSettings();

            LoadMarket.Visibility = Visibility.Hidden;
        }

        private void InitSettings()
        {
            //SetRunLoanFunc(LoanCalculation);
            SetRunLoanFunc(Utilities.RunTheLoanASync);

            Share.shouldShowCriteriaSelectionWindow = false;
            Share.shouldShowCriteriaSelectionContinue = false;
            Share.shouldShowProductSelectionWindow = false;
            Share.shouldShowProductSelectionContinue = false;
            Share.shouldShowRatesSelectionWindow = false;
            Share.shouldShowLoansSelectionWindow = true;
            
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

            // TBD. Should read from env.
            Share.CustomerName = MiscConstants.UNDEFINED_STRING; // "Citi"; // "Clal"; 

            // TBD. Should read from env.
            RunEnvironment.SetMarket(markets.ISRAEL); //  USA);

            // set the settings file accordingly
            //COMBINATIONS_FILE = "Combinations.csv";
            //RISK_LIQUIDITY_FILE = "RiskLiquidityCiti.xlsx";
            //PRODUCTS_FILE = "MortgageProducts - Updated.xml";
            //CRETIRIA_FILE = "Gui.xml";
            //RATES_FILE = "RateFileGeneric.csv";
            //LOAN_FILE = "TestCases.xlsx"; 

    }

        private void SetLogger(MyDelegate func)
        {
            WindowsUtilities.loggerMethod = func;
        }

        private void SetRunLoanFunc(MyRunDelegate func)
        {
            WindowsUtilities.runLoanMethod = func;
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


        public void Log2Window(string msg)
        {
            AddEntry(msg);
        }

         private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Share.numberOfPrintResultsInList = 0;
            Share.shouldRunSync = true;
            SetButtonEnable(false);
            Utilities.RunTheLogic();
        }


        private void AskInput_Button_Click(object sender, RoutedEventArgs e)
        {
            Share.shouldRunSync = true;
            SetButtonEnable(false);
            Utilities.Ask4Input();
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