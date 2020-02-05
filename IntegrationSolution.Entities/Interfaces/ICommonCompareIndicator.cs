using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Interfaces
{
    public interface ICommonCompareIndicator<T> 
    {
        T SAP { get; set; }

        T Wialon { get; set; }

        T Difference { get; }
    }
}
