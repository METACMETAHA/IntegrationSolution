using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WialonBase.Helpers
{
    /// <summary>
    /// Wialon works only with unix formatted date
    /// </summary>
    public static class DateTimeToUnixFormatConverter
    {
        public static Int32 ToUnixTime(this DateTime dateTime)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            TimeSpan span = (dateTime.ToLocalTime() - epoch);
            return (Int32)Math.Round(span.TotalSeconds);
        }
    }
}
