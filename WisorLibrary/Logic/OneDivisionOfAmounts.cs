using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WisorLib
{
    class OneDivisionOfAmounts
    {
        // General Parameters
        public double amtOptX = -1, amtOptY = -1, amtOptZ = -1;
        private bool printOrNo;
        private PmtLimits pmtLimits = null;
        public Option fixedOptZ0 = null;

        // Plane for search
        public SinglePlane plane = null;
        


        // Lists of saved matching points
        public SavedMatchesBeforeCheck savedMatches = new SavedMatchesBeforeCheck();
        private SavedCompositions savedCompositionsForFile = null;


        public OneDivisionOfAmounts(double amtOpt1, double amtOpt2, double amtOpt3, RunEnvironment env)
        {
            Interlocked.Add(ref Share.counterOfOneDivisionOfAmounts, 1);

            printOrNo = env.PrintOptions.printFunctionsInConsole;

            amtOptX = amtOpt1;
            amtOptY = amtOpt2;
            amtOptZ = amtOpt3;

            pmtLimits = new PmtLimits(amtOptX, amtOptY, amtOptZ, env);
            fixedOptZ0 = FindZ0(env.CalculationParameters.optTypes, pmtLimits,
                                    env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.minTime, env);
            if (fixedOptZ0 != null)
            {
                while ((fixedOptZ0 != null) && 
                            (fixedOptZ0.optTime <= env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.maxTime))
                {
                    // Omri - when this loop is ended?
                    plane = new SinglePlane(pmtLimits, fixedOptZ0, env);
                    
                    savedCompositionsForFile = new SavedCompositions(fixedOptZ0, plane.savedMatches, env);
                    
                    if (fixedOptZ0.optTime == env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.maxTime)
                    {
                        fixedOptZ0 = null;
                    }
                    else
                    {
                        fixedOptZ0 = FindZ0(env.CalculationParameters.optTypes, pmtLimits,
                                        (fixedOptZ0.optTime + env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.timeJump), env);
                    }
                }
            }
            else
            {
                if (printOrNo == true)
                {
                    Console.WriteLine("\nZ0 Time does not exist. No possible matches for this division of money\n");
                }
            }
        }






        // **************************************************************************************************************************** //
        // ******************************************************** Find Z0 Plane ***************************************************** //

        private Option FindZ0(OptionTypes optTypes, PmtLimits pmtLimits, uint timeZ, RunEnvironment env)
        {
            Option optionZ0 = null;
            //Console.WriteLine("FindZ0 optTypes: " + optTypes.ToString() + ", pmtLimits: " + pmtLimits.ToString() + ", timeZ: " + timeZ);


            double ttlpmtmaxT = pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                + pmtLimits.opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MAXTIME].optPmt;
            double ttlpmtminT = pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME].optPmt
                                + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME].optPmt
                                + pmtLimits.opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MINTIME].optPmt;

            if ((ttlpmtmaxT > env.CalculationParameters.monthlyPmtWanted + CalculationConstants.smallDev) ||
                    (ttlpmtminT < env.CalculationParameters.monthlyPmtWanted - CalculationConstants.largeDev))
            {
                if (ttlpmtmaxT > env.CalculationParameters.monthlyPmtWanted + CalculationConstants.smallDev)
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine("\nTarget monthly payment " + env.CalculationParameters.monthlyPmtWanted
                                            + " smaller than minimum PMT " + ttlpmtmaxT + "\nNo possible matches for this division of money\n");
                    }
                    optionZ0 = null;
                }
                else if (ttlpmtminT < env.CalculationParameters.monthlyPmtWanted - CalculationConstants.largeDev)
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine("\nTarget monthly payment " + env.CalculationParameters.monthlyPmtWanted
                                            + " larger than maximum PMT " + ttlpmtminT + "\nNo possible matches for this division of money\n");
                    }
                    optionZ0 = null;
                }
                return optionZ0;
            }
            else
            {
                optionZ0 = new Option(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ].product.productID.numberID,
                                            pmtLimits.amts[(int)Options.options.OPTZ], timeZ, env);
                double ttlPMTmaxTime = pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                        + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                        + optionZ0.optPmt;
                double ttlPMTminTime = pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME].optPmt
                                        + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME].optPmt
                                        + optionZ0.optPmt;
                if (printOrNo == true)
                {
                    Console.WriteLine("\nTotal PMT for inserted TimeZ = " + timeZ + " ...\n");
                    Console.WriteLine("(X,Y,Z) = (" + (optTypes.optionTypes[(int)Options.options.OPTX].product.maxTime / 12) + ","
                                        + (optTypes.optionTypes[(int)Options.options.OPTY].product.maxTime / 12) + "," + (timeZ / 12).ToString()
                                        + ") = " + pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                        + " + " + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME].optPmt
                                        + " + " + optionZ0.optPmt + " = " + ttlPMTmaxTime);
                    Console.WriteLine("(X,Y,Z) = (" + (optTypes.optionTypes[(int)Options.options.OPTX].product.minTime / 12) + ","
                                        + (optTypes.optionTypes[(int)Options.options.OPTY].product.minTime / 12) + "," + (timeZ / 12).ToString()
                                        + ") = " + pmtLimits.opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME].optPmt
                                        + " + " + pmtLimits.opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME].optPmt
                                        + " + " + optionZ0.optPmt + " = " + ttlPMTminTime);                                        
                }

                if ((ttlPMTmaxTime > env.CalculationParameters.monthlyPmtWanted + CalculationConstants.smallDev) ||
                                    (ttlPMTminTime < env.CalculationParameters.monthlyPmtWanted - CalculationConstants.largeDev))
                {
                    if (ttlPMTmaxTime > env.CalculationParameters.monthlyPmtWanted + CalculationConstants.smallDev)
                    {
                        if (printOrNo == true)
                        {
                            Console.WriteLine(env.CalculationParameters.monthlyPmtWanted.ToString() + " < " + ttlPMTmaxTime.ToString()
                                                + "Total PMT (max,max," + (timeZ / 12) + ") is too large.");
                        }
                        if (timeZ == optTypes.optionTypes[(int)Options.options.OPTZ].product.maxTime)
                        {
                            if (printOrNo == true)
                            {
                                Console.WriteLine("(X,Y,Z) = (" + (optTypes.optionTypes[(int)Options.options.OPTX].product.maxTime / 12)
                                                    + "," + (optTypes.optionTypes[(int)Options.options.OPTY].product.maxTime / 12) + ","
                                                    + (timeZ / 12) + ") -> too large - no matches can be found. End.");
                            }
                            optionZ0 = null;
                            return optionZ0;
                        }
                        else
                        {
                            if (printOrNo == true)
                            {
                                Console.WriteLine("(X,Y,Z) = (" + (optTypes.optionTypes[(int)Options.options.OPTX].product.maxTime / 12)
                                                    + "," + (optTypes.optionTypes[(int)Options.options.OPTY].product.maxTime / 12) + ","
                                                    + (timeZ / 12) + ") -> too large.\nZ < max -> need to make Z longer ..."
                                                    + "\nNew timeZ for check = " + (timeZ + optTypes.optionTypes[(int)Options.options.OPTZ].product.timeJump));
                            }
                            return FindZ0(optTypes, pmtLimits, (timeZ + optTypes.optionTypes[(int)Options.options.OPTZ].product.timeJump), env);
                        }
                    }
                    else if (ttlPMTminTime < env.CalculationParameters.monthlyPmtWanted - CalculationConstants.largeDev)
                    {
                        if (printOrNo == true)
                        {
                            Console.WriteLine(ttlPMTminTime.ToString() + " < " + env.CalculationParameters.monthlyPmtWanted.ToString()
                                                + "Total PMT (min,min," + (timeZ / 12) + ") is too small.\n"
                                                + "(X,Y,Z) = (" + (optTypes.optionTypes[(int)Options.options.OPTX].product.minTime / 12)
                                                + "," + (optTypes.optionTypes[(int)Options.options.OPTY].product.minTime / 12) + ","
                                                + (timeZ / 12) + ") -> too small - no matches can be found. End.");
                        }
                        optionZ0 = null;
                        return optionZ0;
                    }
                }
                else
                {
                    if (printOrNo == true)
                    {
                        Console.WriteLine(ttlPMTminTime.ToString() + " >= " + env.CalculationParameters.monthlyPmtWanted.ToString()
                                            + " >= " + ttlPMTmaxTime.ToString() + "(X,Y,Z) = ("
                                            + (optTypes.optionTypes[(int)Options.options.OPTY].product.minTime / 12)
                                            + "," + (optTypes.optionTypes[(int)Options.options.OPTY].product.minTime / 12) + ","
                                            + (timeZ / 12).ToString() + ") => Target Payment => (X,Y,Z) = ("
                                            + (optTypes.optionTypes[(int)Options.options.OPTX].product.maxTime / 12)
                                            + "," + (optTypes.optionTypes[(int)Options.options.OPTY].product.maxTime / 12)
                                            + "," + (timeZ / 12).ToString() + ")");
                    }
                    return optionZ0;
                }
                return optionZ0;
            }
        }





    }
}
