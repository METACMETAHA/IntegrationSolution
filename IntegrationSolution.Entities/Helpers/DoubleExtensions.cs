using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Helpers
{
    public static class DoubleExtensions
    {
        public static double GetPercentFrom(this double obj, double number)
        {
            var midSum = (obj + number) / 2;
            var result = ((obj - number) / midSum);
            return Math.Round(Math.Abs(result), 5);
        }
    }
}
