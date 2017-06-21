using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using WisorLib;

namespace WisorLibrary.Utilities
{
    public class XMLUtilities
    {
        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            TextWriter /*XmlWriter*/ writer = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filePath, append);
                //XmlWriterSettings settings = new XmlWriterSettings();
                //settings.OmitXmlDeclaration = true;
                //writer = XmlWriter.Create((filePath, settings);
                var emptyNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
                serializer.Serialize(writer, objectToWrite, emptyNs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR WriteToXmlFile Exception: " + ex.ToString());
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            TextReader reader = null;
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                reader = new StreamReader(filePath);
                return (T)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ReadFromXmlFile Exception: " + ex.ToString());
                return default(T);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        public static void LoadCompositionsXMLFile(string filename, out Composition[] compositions)
        {
            compositions = new Composition[Share.numberOfOption];
            int compositionsIndex = 0;
            int optionsIndex = 0;

            for (int i = 0; i < compositions.Length; i++)
                compositions[i] = new Composition();
 
            try
            {
                if (File.Exists(filename))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filename);
                    XmlNodeList xnList = doc.SelectNodes("Result/Composition");
                    int co = xnList.Count;

                    foreach (XmlNode xn in xnList)
                    {
                        XmlNodeList compDetails = xn.ChildNodes;
                        if (null != compDetails)
                        {
                            Option[] options = new Option[Share.numberOfOption];
                            foreach (XmlNode c in compDetails)
                            {
                                if ("opts" == c.Name)
                                {
                                    XmlNodeList opts = c.SelectNodes("Option");
                                    foreach (XmlNode o in opts)
                                    {
                                        XmlNodeList optDetails = o.ChildNodes;
                                        options[optionsIndex] = new Option();
                                        foreach (XmlNode op in optDetails)
                                        {
                                            SetOptionField(options[optionsIndex], op.Name, op.InnerText);
                                        }
                                        Console.WriteLine("set option data: " + options[optionsIndex].ToString());
                                        optionsIndex = (++optionsIndex) % options.Length;
                                        Console.WriteLine("MOVE optionsIndex: " + optionsIndex);
                                    }
                                }
                                else
                                {
                                    //Console.WriteLine("FOUND NODE: text: " + t + ", name: " + n + ", value: " + v);
                                    SetCompositionField(compositions[compositionsIndex], c.Name, c.InnerText);
                                }
                            }
                            compositions[compositionsIndex].opts = options;
                        }
                        //compositions[compositionsIndex].opts = options;
                        compositionsIndex = (++compositionsIndex) % compositions.Length;
                        Console.WriteLine("MOVE compositionsIndex: " + compositionsIndex);
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("ERROR LoadCompositionsXMLFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("ERROR LoadCompositionsXMLFile Exception occured: " + ex.ToString());
            }
        }

        public static void LoadResultsXMLFile(string filename,
            out loanDetails theLoan, out Composition[] compositions)
        {
            theLoan = null;
            compositions = null;

            LoadCompositionsXMLFile(filename, out compositions);

            LoadLoanDetailsXMLFile(filename, out theLoan);

            // print 
            Console.WriteLine("theLoan: " + theLoan.ToString());
            for (int i = 0; i < compositions.Length; i++)
                Console.WriteLine("compositions " + compositions[i].name + " : " + compositions[i].ToString());
        }

        public static void LoadLoanDetailsXMLFile(string filename,
            out loanDetails theLoan)
        {
            theLoan = new loanDetails();

            try
            {
                if (File.Exists(filename))
                {
                    XDocument doc = XDocument.Load(filename);

                    // loop all the loan-details items in the document
                    foreach (XElement loandetails in from ld in doc.Descendants("loanDetails") select ld)
                    {
                          theLoan = new loanDetails(loandetails.Element("ID").Value, 
                            Convert.ToUInt32(loandetails.Element("OriginalLoanAmount").Value), 
                            Convert.ToUInt32(loandetails.Element("DesiredMonthlyPayment").Value),
                            Convert.ToUInt32(loandetails.Element("PropertyValue").Value), 
                            Convert.ToUInt32(loandetails.Element("YearlyIncome").Value), 
                            Convert.ToUInt32(loandetails.Element("BorrowerAge").Value),
                            Convert.ToInt32(loandetails.Element("fico").Value), 
                            Convert.ToDateTime(loandetails.Element("DateTaken").Value), 
                            null /*ProductID*/, false /*should calculate*/,
                            Convert.ToInt32(loandetails.Element("OriginalInflation").Value));
                        theLoan.resultReportData.RemaingLoanAmount = Convert.ToUInt32(loandetails.Element("LoanAmount").Value);
                        theLoan.DesireTerminationMonth = Convert.ToUInt32(loandetails.Element("DesireTerminationMonth").Value);
                    }
                }
                else
                {
                    WindowsUtilities.loggerMethod("LoadLoanDetailsXMLFile file: " + filename + " does not exists!!!");
                }
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("ERROR LoadLoanDetailsXMLFile Exception occured: " + ex.ToString());
            }

            if (null == theLoan)
            {
                WindowsUtilities.loggerMethod("ERROR LoadLoanDetailsXMLFile failed to load loan-details and compositions from file: " + filename);
            }
            //else
            //{
            //    WindowsUtilities.loggerMethod("LoadLoanDetailsXMLFile succeffuly load: " + compositions.Length + " compositions from file: " +
            //        filename.Substring(filename.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            //}

        }

        static void SetOptionField(Option opt, string key, string value)
        {
            switch (key)
            {
                case "optType":
                    opt.optType = Convert.ToInt32(value);
                    //options[optionsIndex].product = new GenericProduct();
                    //GenericProduct prdct = GenericProduct.GetProduct(options[optionsIndex].optType);
                    //if (null == prdct)
                    //{
                    //    options[optionsIndex].product.productID = new ProductID(
                    //        options[optionsIndex].optType, MiscConstants.UNDEFINED_STRING);
                    //}
                    //else
                    //{
                    //    options[optionsIndex].product.productID = prdct.productID;
                    //}

                    break;
                case "optAmt":
                    opt.optAmt = Convert.ToInt32(value);
                    break;
                case "optTime":
                    opt.optTime = Convert.ToUInt32(value);
                    break;
                case "optPmt":
                    opt.optPmt = Convert.ToInt32(value);
                    break;
                case "optTtlPay":
                    opt.optTtlPay = Convert.ToInt32(value);
                    break;
                case "optRateFirstPeriod":
                    opt.optRateFirstPeriod = Convert.ToDouble(value);
                    break;
                default:
                    break;
            }
        }

        static void SetCompositionField(Composition comp, string key, string value)
        {
            switch (key)
            {
                case "name":
                    comp.name = value;
                    break;
                case "ttlPmt":
                    comp.ttlPmt = Convert.ToInt32(value);
                    break;
                case "ttlPay":
                    comp.ttlPay = Convert.ToUInt32(value);
                    break;
                case "optXBankTtlPay":
                    comp.optXBankTtlPay = Convert.ToUInt32(value);
                    break;
                case "optYBankTtlPay":
                    comp.optYBankTtlPay = Convert.ToUInt32(value);
                    break;
                case "optZBankTtlPay":
                    comp.optZBankTtlPay = Convert.ToUInt32(value);
                    break;
                default:
                    break;
            }
        }


    }
}
