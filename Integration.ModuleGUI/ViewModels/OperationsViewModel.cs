using Integration.ModuleGUI.Models;
using IntegrationSolution.Excel.Interfaces;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class OperationsViewModel : VMLocalBase
    {
        public OperationsViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            this.Title = "Операции";
            this.CanGoBack = true;
            WriteTotalStatisticsInFileCommand = new DelegateCommand(WriteTotalStatistics);
        }


        public override bool MoveBack() => this.CanGoBack;


        public override async Task<bool> MoveNext()
        {
            this.IsFinished = true;
            return CanGoNext;
        }


        #region Commands
        public DelegateCommand WriteTotalStatisticsInFileCommand { get; private set; }
        protected void WriteTotalStatistics()
        {
            try
            {
                var cars = (this.ModuleData.ExcelMainFile as ICarOperations)?.GetVehicles()?.ToList();
                var storageData = (this.ModuleData.ExcelPathListFile as ICarOperations);

                if (cars == null || storageData == null || cars.Count() == 0)
                    throw new Exception("Ошибка. Попробуйте вернуться и загрузить файлы по новой.");

                for (int i = 0; i < cars.Count; i++)
                {
                    var car = cars.ElementAtOrDefault(i);
                    storageData.SetFieldsOfVehicleByAvaliableData(ref car);
                }

                (this.ModuleData.ExcelMainFile as ICarOperations).WriteInHeadersAndDataForTotalResult(cars);
                (this.ModuleData.ExcelMainFile as ICarOperations).WriteInTotalResultOfEachStructure(cars);

                ModuleData.ExcelMainFile.Save();
                this.CanGoNext = true;
            }
            catch (Exception ex)
            {
                this.Error = new IntegrationSolution.Common.Entities.Error()
                {
                    IsError = true,
                    ErrorDescription = ex.Message
                };
            }

            base.NotifyOnUpdateEvents();
        }
        #endregion
    }
}
