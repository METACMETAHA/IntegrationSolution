using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.ModulesExtension.Interfaces
{
    /// <summary>
    /// Each ViewModel which is related to WizzardControl
    /// must be implemented by current interface
    /// </summary>
    public interface IModuleViewModelNavigation
    {
        bool CanGoNext { get; set; }

        bool CanGoBack { get; set; }

        Task<bool> MoveNext();

        bool MoveBack();
    }
}
