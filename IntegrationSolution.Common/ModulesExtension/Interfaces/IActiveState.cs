using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.ModulesExtension.Interfaces
{
    public interface IActiveState
    {
        bool IsActive { get; set; }

        void OnEnter();

        void OnExit();
    }
}
