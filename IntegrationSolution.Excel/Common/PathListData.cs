using IntegrationSolution.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Common
{
    public class PathListData
    {
        public static IDictionary<PathListStatusEnum, string> PathListStatusDictionary { get; private set; } =
            new Dictionary<PathListStatusEnum, string>();

        static PathListData()
        {
            PathListStatusDictionary.Add(PathListStatusEnum.InWork, "ВРАБ");
            PathListStatusDictionary.Add(PathListStatusEnum.Miv, "МІВИ");
            PathListStatusDictionary.Add(PathListStatusEnum.Calc, "РАСЧ");
            PathListStatusDictionary.Add(PathListStatusEnum.Strs, "СТРС");
        }
    }
}
