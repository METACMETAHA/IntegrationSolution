using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Converters
{
    public static class StringToDoubleConverter
    {
        public static double ToDouble(this string str)
        {
            bool containsMinus = str.Contains('-');
            double tmp;

            if (containsMinus)
                str = str.Replace("-", string.Empty);

            if (!double.TryParse(str, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CurrentCulture, out tmp))
                double.TryParse(str.Replace(',', '.'), System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CurrentCulture, out tmp);

            if (containsMinus)
                tmp *= -1;

            return tmp;
        }
    }
}
