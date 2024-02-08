using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{
    public class PricingCalculator
    {
        // Calculation and return of total taxes
        
        public static double CalculateTaxesForCombination(Journey outboundJourney, Journey inboundJourney)
        {
            double totalTaxes = 0;

            
            if (outboundJourney != null)
            {
                totalTaxes += outboundJourney.importTaxAdl + outboundJourney.importTaxChd + outboundJourney.importTaxInf;
            }

            
            if (inboundJourney != null)
            {
                totalTaxes += inboundJourney.importTaxAdl + inboundJourney.importTaxChd + inboundJourney.importTaxInf;
            }

            return Math.Round(totalTaxes, 2);
        }
        //Calculation of Total Price for flight combination
        public static double CalculateFullPriceForCombination(double basePrice, double taxes)
        {
            return Math.Round(basePrice + taxes, 2);
        }
    }


}
