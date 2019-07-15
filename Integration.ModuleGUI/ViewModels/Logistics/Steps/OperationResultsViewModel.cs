using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Entities.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class OperationResultsViewModel : VMLocalBase
    {
        #region Filters
        public Func<object, string, bool> StateNumberVehiclesFilter
        {
            get
            {
                return (item, text) =>
                {
                    var car = item as IVehicleSAP;
                    return car.StateNumber.Contains(text);
                         //|| car..Contains(text);
                };
            }
        }
        #endregion

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";
        }

        public override bool MoveBack()
        {
            //ModuleData.Vehicles
            return true;
        }

        public override async Task<bool> MoveNext()
        {
            return true;
        }
    }
}
