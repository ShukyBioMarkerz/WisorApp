using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WisorLib;
using static WisorAppWpf.SelectionWindow;
using static WisorLib.MiscConstants;

namespace WisorAppWpf
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window
    {

        public SelectionWindow(SelectionType selection)
        {
            Share.theSelectionType = selection;
            InitializeComponent();

            this.DataContext = new MainViewModel();
        }

        private void ContinueProcessing(object sender, RoutedEventArgs e)
        {
            if (SelectionType.ReadCretiria == Share.theSelectionType)
                SaveCriteriaFile();
            else if (SelectionType.ReadProducts == Share.theSelectionType)
                SaveProductsFile();
            else
                WindowsUtilities.loggerMethod("ERROR: ContinueProcessing unrecognized SelectionType: " + Share.theSelectionType.ToString());
        }

        private void SaveCriteriaFile()
        {
            // the user ask to save the criteria definition to file
            // set the filename to the current date
            var dgc = SampleData.Instance.DataGridCollection2;

            //List<string> criteriaList = new List<string>();
            //bool shouldSaveTheOldFile = false;
            //string filename = Share.theCriteriaFilename;
            //string filenm = System.IO.Path.GetFileNameWithoutExtension(filename);
            //string ext = System.IO.Path.GetExtension(filename);
            //string dir = System.IO.Path.GetDirectoryName(filename);
            //string newfn = dir + System.IO.Path.DirectorySeparatorChar + filenm + DateTime.Now.ToString("MM-dd-yyyy-h-mm-tt") + ext;

            //if (shouldSaveTheOldFile)
            //{
            //    // rename the old one
            //    try
            //    {
            //        if (File.Exists(filename))
            //        {
            //            File.Move(filename, newfn);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // TBD use the WindowsUtilities.loggerMethod
            //        WindowsUtilities.GetLogger()("ERROR: File.Move got Exception from file: " + filename + " to file: " + newfn + ". Exception: " + ex.ToString());
            //    }
            //}

            int index = 0;
            FieldList fl = new FieldList();
            foreach (DataGridRowModel dg in dgc)
            {
                fl.Add(new CriteriaField(dg.ID, index++));
                //criteriaList.Add(dg.ID);
            }
            Share.theSelectedCriteriaFields = fl;

            // Reorder the cretiria data
            Share.OrderTheCriteriaFields();

            //File.WriteAllLines(filename, criteriaList);

            // close the window
            this.Close();
        }


        private void SaveProductsFile()
        {
            // the user ask to save the criteria definition to file
            // set the filename to the current date
            var dgc = SampleData.Instance.DataGridCollection2;

            ProductsList products = new ProductsList();

            foreach (DataGridRowModel dg in dgc)
            {
                int index = GenericProduct.GetProductIndex(dg.ID);
                products.Add(index, new GenericProduct(new ProductID(index, dg.ID)));
            }

            // save the list in the global var
            Share.theSelectedProducts = products;
            // Reorder the cretiria data
            Share.OrderTheProductsFields();

            // close the window
            this.Close();

        }

    }


    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    //////////////////////////////////////////////

    public class MainViewModel : ViewModelBase
    {
        private SampleData _data;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.Data = SampleData.Instance; //    new SampleData.;
        }

        public SampleData Data
        {
            get { return _data; }
            set
            {
                if (Equals(value, _data)) return;
                _data = value;
                OnPropertyChanged();
            }
        }

    }


}
