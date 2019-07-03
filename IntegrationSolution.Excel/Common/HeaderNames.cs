using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace IntegrationSolution.Excel
{
    public class HeaderNames
    {
        private readonly SerializeConfigDTO _configDTO;

        #region Headers from first file
        public string PathListType = "Вид путевого листа";
        public string PathListStatus = "Наименование статуса путевого листа";

        public string UnitNumber = "Номер единицы оборудования";
        /// <summary>
        /// From first file
        /// </summary>
        public string UnitModel = "Марка единицы оборудования";
        public string StateNumber = "Гос.знак единицы оборудования";
        public string NumberOfDriver = "Табельный номер водителя";
        public string FullNameOfDriver = "ФИО водителя";
        public string NumberOfCustomer = "Табельный номер заказчика";
        public string FullNameOfCustomer = "ФИО заказчика";
               
        public string DepartureFromGarageDate = "Дата выезда из гаража (факт)";
        public string DepartureFromGarageTime = "Время выезда из гаража (факт)";
        public string ReturnToGarageDate = "Дата возвращения в гараж (факт)";
        public string ReturnToGarageTime = "Время возвращения в гараж (факт)";
        public string TimeOnDutyAtAll = "Время в наряде (всего)";
               
        public string DepartureOdometerValue = "Показания одометра (выезд)";
        public string ReturnOdometerValue = "Показания одометра (возврат)";
        public string TotalMileage = "Общий пробег";
        public string DepartureMotoHoursIndications = "Показания моточасов (выезд)";
        public string ReturnMotoHoursIndications = "Показания моточасов (возврат)";
        public string MotoHoursIndicationsAtAll = "Моточасов всего";

        public string CountTrips = "Количество маршрутов в ПЛ";
        public string DriverWorkMode = "Режим работы водителя";
        public string DriverWorkModeDescription = "Режим работы водителя - текст";
               
        public string DepartmentCode = "Служба";
        public string DepartmentTitle = "Наименование Службы";
        
        #region Fuel
        public string DepartureBalanceGas = "Бензин Остаток при Выезде 1";
        public string ReturnBalanceGas = "Бензин Остаток при Возвращении 1";
        public string ConsumptionGasActual = "Бензин Фактический расход 1";
        public string ConsumptionGasNormative = "Бензин Нормативный расход 1";
        public string ConsumptionGasSavingsOrOverruns = "Бензин Экономия/Перерасход 1";
               
        public string DepartureBalanceDisel = "ДТ Остаток при Выезде 2";
        public string ReturnBalanceDisel = "ДТ Остаток при Возвращении 2";
        public string ConsumptionDiselActual = "ДТ Фактический расход 2";
        public string ConsumptionDiselNormative = "ДТ Нормативный расход 2";
        public string ConsumptionDiselSavingsOrOverruns = "ДТ Экономия/Перерасход 2";
               
        public string DepartureBalanceLPG = "Газ Остаток при Выезде 3";
        public string ReturnBalanceLPG = "Газ Остаток при Возвращении 3";
        public string ConsumptionLPGActual = "Газ Фактический расход 3";
        public string ConsumptionLPGNormative = "Газ Нормативный расход 3";
        public string ConsumptionLPGSavingsOrOverruns = "Газ Экономия/Перерасход 3";
        #endregion Fuel
        #endregion


        #region Headers from second file
        public string Departments = "Службы/отделы";
        public string TypeOfVehicle = "Тип";
        public string PartOfStructureNameForResult = "Структурные подразделени";
        /// <summary>
        /// From second file
        /// </summary>
        public string ModelOfVehicle = "Марка";
        #endregion


        #region Headers to result first file
        public string TotalMileageResult = "Общий пробег за период, км";
        public string TotalJobDoneResult = "Общая наработка мото/час";
        public string ConsumptionGasActualResult = "Бензин Фактический расход, л";
        public string ConsumptionDieselActualResult = "ДТ Фактический расход, л.";
        public string ConsumptionLPGActualResult = "Газ Фактический расход, л";
        #region Cost Headers
        public string Amortization = "Амортизация";
        public string DriversFOT = "ФОТ водителя, грн.";
        public string TotalCost = "Всего затрат, грн.";
        public string TotalCostDisel = "Затраты на топливо (ДТ), грн без НДС";
        public string TotalCostGas = "Затраты на топливо (бензин), грн без НДС";
        public string TotalCostLPG = "Затраты на топливо (ГАЗ), грн без НДС";
        #endregion
        #endregion


        public Dictionary<string, string> PropertiesData { get; set; }


        public HeaderNames(SerializeConfigDTO configDTO) 
        {
            _configDTO = configDTO;
            PropertiesData = GetFieldValues(this);
            InitializeHeaders(_configDTO.HeaderNamesChanged);
        }

        public Dictionary<string, string> GetFieldValues(object obj)
        {
            return obj.GetType()
                      .GetFields()
                      .Where(f => f.FieldType == typeof(string))
                      .ToDictionary(f => f.Name,
                                    f => (string)f.GetValue(this));
        }


        public string GetFieldValueByPropName(object obj, string property)
        {
            return (string)obj.GetType()
                      .GetField(property)
                      .GetValue(this);
        }


        public void InitializeHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
                return;

            foreach (var item in headers)
            {
                if (PropertiesData.ContainsKey(item.Key))
                    PropertiesData[item.Key] = item.Value;
            }
        }
    }
}
