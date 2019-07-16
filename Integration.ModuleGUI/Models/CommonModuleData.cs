using IntegrationSolution.Entities.Implementations.Wialon;
using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity;
using Unity.Resolution;

namespace Integration.ModuleGUI.Models
{
    public class CommonModuleData : BindableBase
    {
        protected readonly IUnityContainer _container;
        protected readonly HeaderNames _headerNames;

        #region Properties
        private string _pathToMainFile;
        public string PathToMainFile
        {
            get { return _pathToMainFile; }
            set { SetProperty(ref _pathToMainFile, value); }
        }


        private string _pathToPathListFile;
        public string PathToPathListFile
        {
            get { return _pathToPathListFile; }
            set
            { SetProperty(ref _pathToPathListFile, value); }
        }


        private IExcel _excelMainFile;
        public IExcel ExcelMainFile
        {
            get { return _excelMainFile; }
            set
            { SetProperty(ref _excelMainFile, value); }
        }


        private IExcel _excelPathListFile;
        public IExcel ExcelPathListFile
        {
            get { return _excelPathListFile; }
            set
            { SetProperty(ref _excelPathListFile, value); }
        }

        #region Vehicles
        private ObservableCollection<IVehicleSAP> vehicles;
        public ObservableCollection<IVehicleSAP> Vehicles
        {
            get { return vehicles; }
            set { SetProperty(ref vehicles, value); }
        }

        private ObservableCollection<CarWialon> vehiclesNavigate;
        public ObservableCollection<CarWialon> VehiclesNavigate
        {
            get { return vehiclesNavigate; }
            set { SetProperty(ref vehiclesNavigate, value); }
        }

        // Vehicles from excel which are not found in Wialon
        private ObservableCollection<IVehicleSAP> vehiclesExcelDistinctWialon;
        public ObservableCollection<IVehicleSAP> VehiclesExcelDistinctWialon
        {
            get { return vehiclesExcelDistinctWialon; }
            set { SetProperty(ref vehiclesExcelDistinctWialon, value); }
        }

        // Vehicles from Wialon which are not found in excel
        private ObservableCollection<CarWialon> vehiclesWialonDistinctExcel;
        public ObservableCollection<CarWialon> VehiclesWialonDistinctExcel
        {
            get { return vehiclesWialonDistinctExcel; }
            set { SetProperty(ref vehiclesWialonDistinctExcel, value); }
        }
        #endregion

        #endregion Properties


        public CommonModuleData(IUnityContainer container)
        {
            _container = container;
            _headerNames = _container.Resolve<HeaderNames>();
        }


        /// <summary>
        /// Trying create IExcel objects from pathes to files.
        /// If documents have invalid structure - it returns Exception else null.
        /// </summary>
        /// <returns></returns>
        public Exception TryCreateObject()
        {
            try
            {
                ExcelPackage eMain = new ExcelPackage(new System.IO.FileInfo(this.PathToMainFile));
                ExcelPackage ePathList = new ExcelPackage(new System.IO.FileInfo(this.PathToPathListFile));

                ExcelMainFile = (IExcel)_container.Resolve<ICarOperations>(new ResolverOverride[] { new ParameterOverride("excelPackage", eMain) });
                var headers = IntegrationSolution.Excel.Common.StaticHelper.GetHeadersAddress((ExcelBase)ExcelMainFile
                    , _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]
                    , _headerNames.PropertiesData[nameof(_headerNames.TypeOfVehicle)]
                    , _headerNames.PropertiesData[nameof(_headerNames.Departments)]
                    , _headerNames.PropertiesData[nameof(_headerNames.ModelOfVehicle)]);
                if (headers.Count != 4)
                    throw new Exception($"Неправильная структура \"{this.PathToMainFile}\" документа.\nТребуются следующие колонки:\" " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.StateNumber)]}, " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.TypeOfVehicle)]}, " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.Departments)]}, " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.ModelOfVehicle)]}\"");

                ExcelPathListFile = (IExcel)_container.Resolve<ICarOperations>(new ResolverOverride[] { new ParameterOverride("excelPackage", ePathList) });
                headers = IntegrationSolution.Excel.Common.StaticHelper.GetHeadersAddress((ExcelBase)ExcelPathListFile
                    , _headerNames.PropertiesData[nameof(_headerNames.StateNumber)]
                    , _headerNames.PropertiesData[nameof(_headerNames.TotalMileage)]);
                if (headers.Count != 2)
                    throw new Exception($"Неправильная структура \"{this.PathToPathListFile}\" документа.\nТребуются следующие колонки:\" " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.StateNumber)]}, " +
                    $"{_headerNames.PropertiesData[nameof(_headerNames.TotalMileage)]}\"");
            }
            catch (Exception ex)
            { return ex; }

            return null;
        }
    }
}
