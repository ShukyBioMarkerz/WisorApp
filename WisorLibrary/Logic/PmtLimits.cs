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

        public OneOption(OneOptType optType, double optAmt)
        {
            times[(int)Options.pmtLimits.MAXTIME] = new Option(optType.product.ID, optAmt, optType.product.maxTime);
            times[(int)Options.pmtLimits.MINTIME] = new Option(optType.product.ID, optAmt, optType.product.minTime);
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
                                                amts[(int)Options.options.OPTX]);
            opts[(int)Options.options.OPTY] = new OneOption(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTY],
                                                amts[(int)Options.options.OPTY]);
            opts[(int)Options.options.OPTZ] = new OneOption(env.CalculationParameters.optTypes.optionTypes[(int)Options.options.OPTZ],
                                                amts[(int)Options.options.OPTZ]);
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
