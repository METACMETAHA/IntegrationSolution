using IntegrationSolution.Localization.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WialonBase.Helpers
{
    public class WialonExceptions
    {
        // Messages are resolved at call time (through the lambdas), so they follow the current UI language.
        private static readonly Dictionary<int, Func<string>> _exceptionsInfoByCode = new Dictionary<int, Func<string>>
        {
            {0, () => Strings.Wialon_Error0 },
            {1, () => Strings.Wialon_Error1 },
            {2, () => Strings.Wialon_Error2 },
            {3, () => Strings.Wialon_Error3 },
            {4, () => Strings.Wialon_Error4 },
            {5, () => Strings.Wialon_Error5 },
            {6, () => Strings.Wialon_ErrorUnknown },
            {7, () => Strings.Wialon_Error7 },
            {8, () => Strings.Wialon_Error8 },
            {9, () => Strings.Wialon_Error9 },
            {10, () => Strings.Wialon_Error10 },
            {11, () => Strings.Wialon_Error11 },
            {1001, () => Strings.Wialon_Error1001 },
            {1002, () => Strings.Wialon_Error1002 },
            {1003, () => Strings.Wialon_Error1003 },
            {1004, () => Strings.Wialon_Error1004 },
            {1005, () => Strings.Wialon_Error1005 },
            {1011, () => Strings.Wialon_Error1011 },
            {2014, () => Strings.Wialon_Error2014 },
            {2015, () => Strings.Wialon_Error2015 },
        };


        public static string GetErrorMsg(int code)
        {
            Func<string> getMessage;
            if (_exceptionsInfoByCode.TryGetValue(code, out getMessage))
                return getMessage();

            return Strings.Wialon_ErrorUnknown;
        }
    }
}
