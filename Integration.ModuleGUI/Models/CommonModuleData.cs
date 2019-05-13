using IntegrationSolution.Entities.Interfaces;
using IntegrationSolution.Excel;
using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using OfficeOpenXml;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using Unity.Resolution;
using WialonBase.Entities;

namespace Integration.ModuleGUI.Models
{
    public class CommonModuleData : BindableBase
    {
        protected IUnityContainer _container;

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
        public ICollection<IVehicle> Vehicles { get; set; }
        public ICollection<CarWialon> VehiclesNavigate { get; set; }

        // Vehicles from excel which are not found in Wialon
        public ObservableCollection<IVehicle> VehiclesExcelDistinctWialon { get; set; }

        // Vehicles from Wialon which are not found in excel
        public ObservableCollection<CarWialon> VehiclesWialonDistinctExcel { get; set; }
        #endregion

        #endregion Properties


        public CommonModuleData(IUnityContainer container)
        {
            _container = container;
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
                    HeaderNames.StateNumber, HeaderNames.TypeOfVehicle, HeaderNames.Departments, HeaderNames.ModelOfVehicle);
                if (headers.Count != 4)
                    throw new Exception($"Неправильная структура \"{this.PathToMainFile}\" документа.");

                ExcelPathListFile = (IExcel)_container.Resolve<ICarOperations>(new ResolverOverride[] { new ParameterOverride("excelPackage", ePathList) });
                headers = IntegrationSolution.Excel.Common.StaticHelper.GetHeadersAddress((ExcelBase)ExcelPathListFile,
                    HeaderNames.StateNumber, HeaderNames.NumberOfDriver, HeaderNames.FullNameOfDriver, HeaderNames.TotalMileage);
                if (headers.Count != 4)
                    throw new Exception($"Неправильная структура \"{this.PathToPathListFile}\" документа.");
            }
            catch (Exception ex)
            { return ex; }

            return null;
        }
    }
}
