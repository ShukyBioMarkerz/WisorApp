using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using WisorLibrary.Utilities;
using static WisorLib.MiscConstants;

namespace WisorLibrary.DataObjects
{
    public class HistoricRate
    {
        // make the class singltone
        private static HistoricRate instance;
        string filename;
        public bool Status { get; internal set; }
        DateValueCollection historicData;

        public static bool SetFilename(string filename)
        {
            if (null == instance || !instance.Status)
            {
                instance = new HistoricRate(filename);
            }
            return instance.Status;
        }

        public static HistoricRate Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new HistoricRate();
                }
                return instance;
            }
        }

        HistoricRate(string filename = "")
        {
            Status = false;

            if (!String.IsNullOrEmpty(filename))
            {
                setFilename(filename);
                historicData = LoadHistoricRatesCSVFile(filename);
                if (null != historicData && 0 < historicData.Capacity)
                {
                    Status = true;
                }
                else {
                    Status = false;
                    WindowsUtilities.loggerMethod("ERROR HistoricRate failed to load borrower rates from file: " + filename);
                }
             }
            else
            {
                WindowsUtilities.loggerMethod("NOTICE HistoricRate without setting the rates file name");
            }
        }

        private void setFilename(string fn)
        {
            filename = fn;
        }


        public static double GetHistoricIndex(indices indic, DateTime dateLoanTaken)
        {
            double index = 0;

            if (null != Instance.historicData)
            {
                DateValue dv = Instance.historicData.GetNearest(dateLoanTaken);
                index = dv.Value;
                
            }
            return index;
        }

        public static double GetHistoricValues(indices indic, DateTime fromDate, DateTime toDate)
        {
            double index = 0;
            // Instance.historicData.FindAll(Predicate < DateTime > pred);
            if (null != Instance.historicData)
            {
                index = Instance.historicData.GetValueFromTo(fromDate, toDate);
            }
            return index;
        }

        private DateValueCollection LoadHistoricRatesCSVFile(string filename)
        {
            DateValueCollection historicDate = null;
            string line = MiscConstants.UNDEFINED_STRING;

            try
            {
                string ext = Path.GetExtension(filename);
                string[] lines = null;

                if (".xls" == ext || ".xlsx" == ext)
                {
                    lines = ExcelUtilities.GetLinesFromFile(filename, false /*shouldRemoveFractions*/);
                }
                else if (".csv" == ext)
                {
                    lines = CSVUtilities.GetLinesFromFile(filename);
                }
                if (null == lines || 0 >= lines.Length)
                {
                    Console.WriteLine("ERROR: LoadHistoricRatesCSVFile failed to load from file: " + filename);
                    return null;
                }

                historicDate = new DateValueCollection();
                string[] entities;
                
                int lineNumber = 1;
                double theRate = MiscConstants.UNDEFINED_DOUBLE;
                DateTime? theDate = null;

                for (int li = 0; li < lines.Length; li++)
                {
                    line = lines[li];
                    if (String.IsNullOrEmpty(line))
                        continue;

                    // skip the first line
                    // TBD: should read the headers and relate to it
                    if (1 == lineNumber++)
                        continue;

                    entities = line.Split(MiscConstants.COMMA_SEERATOR_STR);
                    if (String.IsNullOrEmpty(entities[0]))
                    {
                        continue;
                    }
                    //theDate = DateTime.Parse(entities[0]);
                    theDate = MiscUtilities.ConvertDate(entities[0]);

                    //// ensure the line correctness. 2 are the product name and the profile
                    //if (MiscConstants.NumberOfYearsFrProduct + 2 != entities.Length)
                    //{
                    //    continue;
                    //}

                    // clean all redundant chars e.g. %
                    for (int i = 1; i < entities.Length; i++)
                    {
                        int index = entities[i].IndexOf(MiscConstants.PERCANTAGE_STR);
                        string trimed = (0 < index) ? entities[i].Remove(index).Trim() : entities[i].Trim();
                        theRate = Double.Parse(trimed);
                    }

                    if (null != theDate)
                        historicDate.Add(new DateValue((DateTime)theDate, theRate));
                }
            }
            catch (Exception ex)
            {
                WindowsUtilities.loggerMethod("ERROR: LoadHistoricRatesCSVFile got Exception: " + ex.ToString() + ". line: " + line);
            }

            return historicDate;
        }


    }


    /// <summary>
    /// ///////////////////
    /// </summary>
    public struct DateValue
    {
        public DateValue(DateTime date, double val)
        {
            this.Date = date;
            this.Value = val;
            Weight = 0;
        }
        public DateTime Date { get; set; }
        public double Value { get; set; }
        public double Weight { get; set; }

        public override string ToString()
        {
            // cleanup the minutes
            string str = Date.ToString();
            int ind = str.IndexOf(MiscConstants.SPACE_STR);
            if (0 < ind)
                str = str.Remove(str.IndexOf(MiscConstants.SPACE_STR)).Trim();

            return "date: " + str + ", Value: " + Value + ", Weight: " + Weight;
        }
    }

    public class DateValueCollection : List<DateValue>, IComparer<DateValue>
    {
        public DateValueCollection() { }

        public DateValueCollection(IEnumerable<DateValue> dateValues, bool isOrdered)
        {
            if (isOrdered)
                base.AddRange(dateValues);
            else
                base.AddRange(dateValues.OrderBy(dv => dv.Date));
        }

        public DateValue GetNearest(DateTime date)
        {
            if (base.Count == 0)
                return default(DateValue);

            DateValue dv = new DateValue(date, 0);
            int index = base.BinarySearch(dv, this);

            if (index >= 0)
            {
                return base[index];
            }
            // If not found, List.BinarySearch returns the complement of the index
            index = ~index;

            DateValue[] all;
            if (index >= base.Count - 1)
            {
                // proposed index is last, check previous and last
                all = new[] { base[base.Count - 1], base[base.Count - 2] };
            }
            else if (index == 0)
            {
                // proposed index is first, check first and second
                all = new[] { base[index], base[index + 1] };
            }
            else
            {
                // return nearest DateValue from previous and this
                var thisDV = base[index];
                var prevDV = base[index - 1];
                all = new[] { thisDV, prevDV };
            }
            return all.OrderBy(x => (x.Date - date).Duration()).First();
        }

        public double GetValueFromTo(DateTime fromDate, DateTime toDate)
        {
            bool debug = false;

            if (base.Count == 0)
                return 0;

            DateValue dv = new DateValue(fromDate, 0);
            int index = base.BinarySearch(dv, this);
            int minIndex;

            if (index < 0)
            {
                // If not found, List.BinarySearch returns the complement of the index
                index = ~index;
            }

            //DateValue[] all;
            List<DateValue> Values = new List<DateValue>();

            if (index >= base.Count - 1)
            {
                // proposed index is last, check previous and last
                //all = new[] { base[base.Count - 1], base[base.Count - 2] };
                minIndex = base.Count - 2;
            }
            else if (index == 0)
            {
                // proposed index is first, check first and second
                //all = new[] { base[index], base[index + 1] };
                minIndex = index;
            }
            else
            {
                // return nearest DateValue from previous and this
                //var thisDV = base[index];
                //var prevDV = base[index - 1];
                //all = new[] { thisDV, prevDV };
                minIndex = index - 1;
            }

            // loop to find all elements in the dates interval
            //bool founded = false;
            int dates = MiscUtilities.CalculateDatesBetweenDates(fromDate, toDate);
            DateTime lastCalculate = fromDate;
            DateValue current, next;
            int diffDays, leftDays = dates;
           
            do
            {
                if (base.Count <= minIndex+1)
                    break;
                current = base[minIndex];
                next = base[minIndex+1];
                diffDays = MiscUtilities.IsDateInBetween(next.Date, fromDate, toDate);
                // need the range before and after the dates to get the entire duration
                // diffDays = MiscUtilities.CalculateDatesBetweenDates(lastCalculate, current.Date);
                if (0 <= diffDays) // in the range
                {
                    // are we in the next month already?
                    if (diffDays > leftDays)
                        diffDays = leftDays;
                    leftDays -= diffDays;

                    current.Weight = (double)diffDays / dates;
                    Values.Add(current);
                    if (debug)
                    {
                        Console.WriteLine("+++ GetValueFromTo add: " + current.ToString() + ", days: " + diffDays);
                    }
                }
                else {
                    if (0 < leftDays)
                    {
                        current.Weight = (double)leftDays / dates;
                        Values.Add(current);
                        leftDays = 0;
                    }
                }
                
                minIndex++;
                lastCalculate = next.Date;
            } while (/*founded || */ 0 < leftDays);

            double calculatedRate = CalculateAverageRate(Values.ToArray(), fromDate, toDate);
            return calculatedRate;
            //return all.OrderBy(x => (x.Date - fromDate).Duration()).First();
        }

        private double CalculateAverageRate(DateValue[] Values, DateTime fromDate, DateTime toDate)
        {
            double rate = 0;

            for (int i = 0; i < Values.Length; i++)
            {
                rate += Values[i].Value * Values[i].Weight;
            }
            
            return rate;
        }

        public int Compare(DateValue x, DateValue y)
        {
            return x.Date.CompareTo(y.Date);
        }

       
    }


}
