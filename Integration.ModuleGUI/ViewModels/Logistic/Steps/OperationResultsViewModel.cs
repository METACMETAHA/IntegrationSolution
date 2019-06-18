using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using Integration.ModuleGUI.Views.OperationResultsViews;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class OperationResultsViewModel : VMLocalBase
    {
        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";
        }

        public override bool MoveBack()
        {
            //ModuleData.Vehicles.First().
            return true;
        }

        public override async Task<bool> MoveNext()
        {
            return true;
        }
    }
}
