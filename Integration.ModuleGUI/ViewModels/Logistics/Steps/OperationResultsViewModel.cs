using Integration.ModuleGUI.Models;
using Integration.ModuleGUI.Views;
using IntegrationSolution.Common.Helpers;
using IntegrationSolution.Entities.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections;
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
        private IEnumerable filter;
        public IEnumerable FilterDepartment
        {
            get { return filter; }
            set { SetProperty(ref filter, value); }
        }

        public OperationResultsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            FilterDepartment = new List<string>()
            {
                "ss", "dsd"
            };

            CanGoBack = true;
            CanGoNext = true;
            this.Title = "Результаты";
        }

        public override void OnEnter()
        {
            base.OnEnter();
            //FilterDepartment = new ObservableCollection<string>(ModuleData.Vehicles?.ToLookup(x => x.Department).Select(x => x.Key).ToList());
        }

        public override bool MoveBack()
        {
            //ModuleData.Vehicles.First().Trips.Count
            return true;
        }
        
        public override async Task<bool> MoveNext()
        {
            return true;
        }
    }
}
