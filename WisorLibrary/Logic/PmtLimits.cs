using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WisorLib
{
    class OneOption
    {
        // General Parameters
        public Option[] times = { null, null };

        public OneOption(OneOptType optType, double optAmt, RunEnvironment env)
        {
            times[(int)Options.pmtLimits.MAXTIME] = new Option(optType.product.productID.numberID, optAmt, optType.product.maxTime, env);
            if (optType.product.maxTime == optType.product.minTime)
            {
                times[(int)Options.pmtLimits.MINTIME] = times[(int)Options.pmtLimits.MAXTIME];
            }
            else
            {
                times[(int)Options.pmtLimits.MINTIME] = new Option(optType.product.productID.numberID, optAmt, optType.product.minTime, env);
            }
        }
    }



    class PmtLimits
    {
        public double[] amts = { -1, -1, -1 };
        public OneOption[] opts = { null, null, null };

        public PmtLimits(double n_optXAmt, double n_optYAmt, double n_optZAmt, RunEnvironment env)
        {
            amts[(int)Options.options.OPTX] = n_optXAmt;
            amts[(int)Options.options.OPTY] = n_optYAmt;
            amts[(int)Options.options.OPTZ] = n_optZAmt;

            opts[(int)Options.options.OPTX] = new OneOption(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTX],
                                                amts[(int)Options.options.OPTX], env);
            opts[(int)Options.options.OPTY] = new OneOption(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY],
                                                amts[(int)Options.options.OPTY], env);
            opts[(int)Options.options.OPTZ] = new OneOption(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ],
                                                amts[(int)Options.options.OPTZ], env);
        }


        public override string ToString()
        {
            return "OptX: " + opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME] + "="
                        + opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME].optPmt + " , "
                        + opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME] + "="
                        + opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME].optPmt +
                   "\nOptY: " + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME] + "="
                        + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME].optPmt + " , "
                        + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME] + "="
                        + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME].optPmt +
                   "\nOptZ: " + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MAXTIME] + "="
                        + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MAXTIME].optPmt + " , "
                        + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MINTIME] + "="
                        + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MINTIME].optPmt +
                    "\n\t\t" + (opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MAXTIME].optPmt
                        + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MAXTIME].optPmt
                        + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MAXTIME].optPmt) + " <= PMT <= " +
                        (opts[(int)Options.options.OPTX].times[(int)Options.pmtLimits.MINTIME].optPmt
                        + opts[(int)Options.options.OPTY].times[(int)Options.pmtLimits.MINTIME].optPmt
                        + opts[(int)Options.options.OPTZ].times[(int)Options.pmtLimits.MINTIME].optPmt) + "\n";
        }
    }


}
