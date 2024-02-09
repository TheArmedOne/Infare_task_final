using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infare_task_final
{
    // Handles calculations related to pricing and taxes for flight combinations.
    public class PricingCalculator
    {
        // Calculates total taxes for a given flight combination.

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
        // Calculates the total price for a flight combination, including base price and taxes.
        public static double CalculateFullPriceForCombination(double basePrice, double taxes)
        {
            return Math.Round(basePrice + taxes, 2);
        }
    }


}
