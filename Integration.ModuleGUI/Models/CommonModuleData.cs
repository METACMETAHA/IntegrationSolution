﻿using IntegrationSolution.Entities.Implementations.Wialon;
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
        public ICollection<IVehicleSAP> Vehicles { get; set; }
        public ICollection<CarWialon> VehiclesNavigate { get; set; }

        // Vehicles from excel which are not found in Wialon
        public ObservableCollection<IVehicleSAP> VehiclesExcelDistinctWialon { get; set; }

        // Vehicles from Wialon which are not found in excel
        public ObservableCollection<CarWialon> VehiclesWialonDistinctExcel { get; set; }
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
                var headers = IntegrationSolution.Excel.Common.StaticHelper.GetHeadersAddress((ExcelBase)ExcelMainFile,
                    _headerNames.StateNumber, _headerNames.TypeOfVehicle, _headerNames.Departments, _headerNames.ModelOfVehicle);
                if (headers.Count != 4)
                    throw new Exception($"Неправильная структура \"{this.PathToMainFile}\" документа.\nТребуются следующие колонки:\" " +
                    $"{_headerNames.StateNumber}, {_headerNames.TypeOfVehicle}, {_headerNames.Departments}, {_headerNames.ModelOfVehicle}\"");

                ExcelPathListFile = (IExcel)_container.Resolve<ICarOperations>(new ResolverOverride[] { new ParameterOverride("excelPackage", ePathList) });
                headers = IntegrationSolution.Excel.Common.StaticHelper.GetHeadersAddress((ExcelBase)ExcelPathListFile,
                    _headerNames.StateNumber, _headerNames.TotalMileage);
                if (headers.Count != 2)
                    throw new Exception($"Неправильная структура \"{this.PathToPathListFile}\" документа.\nТребуются следующие колонки:\" " +
                    $"{_headerNames.StateNumber}, {_headerNames.TotalMileage}\"");
            }
            catch (Exception ex)
            { return ex; }

            return null;
        }
    }
}
