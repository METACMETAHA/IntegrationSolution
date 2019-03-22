using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Converters
{
    public static class StringToDoubleConverter
    {
        public static double ToDouble(this string str)
        {
            double tmp;
            double.TryParse(str.Replace('.', ','), out tmp);
            return tmp;
        }
    }
}
