using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WisorLib;
using static WisorLib.MiscConstants;

namespace WisorLibrary.Utilities
{
    public class AnalayzeParameters
    {
        static public void AnalayzeCretiriaParameters(string paramText, string paramName,
            ref uint loanAmount, ref uint desiredMonthlyPayment, ref uint propertyValue,
            ref uint yearlyIncome, ref uint borrowerAge, ref int fico, ref DateTime dateTaken,
            ref uint desireTerminationMonth, ref uint sequentialNumber,
            ref double originalRate, ref double originalRate2,
            ref double originalMargin, ref double originalMargin2, ref uint originalTime,
            ref Risk risk, ref Liquidity liquidity, ref ProductID product)
        {
            string txt = paramText.Replace(MiscConstants.COMMA_STR, MiscConstants.UNDEFINED_STRING);  // cleanup
            switch (paramName.ToLower())
            {
                case MiscConstants.LOAN_AMOUNT:
                    loanAmount = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.MONTHLY_PAYMENT:
                    desiredMonthlyPayment = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.PROPERTY_VALUE:
                    propertyValue = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.YEARLY_INCOME:
                    yearlyIncome = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.AGE:
                    borrowerAge = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.LOAN_FICO:
                    fico = Convert.ToInt32(txt);
                    break;
                case MiscConstants.CUSTOMER_NAME:
                    if (!String.IsNullOrEmpty(txt))
                        Share.CustomerName = txt;
                    break;
                case MiscConstants.DATE_TAKEN:
                    DateTime value;
                    if (!DateTime.TryParse(txt, out value))
                    {
                        string[] formats = { "MM/dd/yyyy" };
                        if (!DateTime.TryParseExact(txt, formats, new CultureInfo("en-US"),
                               DateTimeStyles.None, out value))
                        {
                            dateTaken = DateTime.Now;
                        }
                        else
                        {
                            dateTaken = value;
                        }
                    }
                    else
                    {
                        dateTaken = value;
                    }
                    break;
                case MiscConstants.DESIRE_TERMINATION_MONTH:
                    desireTerminationMonth = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.SEQ_NUMBER:
                    sequentialNumber = Convert.ToUInt32(txt);
                    break;
                //case MiscConstants.ORIGINAL_PRODUCT:
                //    originalProduct = txt;
                //    break;
                case MiscConstants.ORIGINAL_RATE:
                    originalRate = Convert.ToDouble(txt);
                    break;
                case MiscConstants.ORIGINAL_RATE2:
                    originalRate2 = Convert.ToDouble(txt);
                    break;
                case MiscConstants.ORIGINAL_MARGIN:
                    originalMargin = Convert.ToDouble(txt);
                    break;
                case MiscConstants.ORIGINAL_MARGIN2:
                    originalMargin2 = Convert.ToDouble(txt);
                    break;
                case MiscConstants.ORIGINAL_TIME:
                    originalTime = Convert.ToUInt32(txt);
                    break;
                case MiscConstants.RISK_VALUE:
                    risk = (Risk)Enum.Parse(typeof(Risk), txt, true);
                    break;
                case MiscConstants.LIQUIDITY_VALUE:
                    liquidity = (Liquidity)Enum.Parse(typeof(Liquidity), txt, true);
                    break;
                case MiscConstants.PRODUCT_NAME:
                    product = new ProductID(MiscConstants.UNDEFINED_INT, txt);
                    break;
                default:
                    Console.WriteLine("StartButton_Clicked Illegal control name: " + name);
                    break;
            }

        }
    }
}
