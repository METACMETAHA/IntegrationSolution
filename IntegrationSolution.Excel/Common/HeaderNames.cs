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
        public string PathListType { get; private set; } = "Вид путевого листа";
        public string PathListStatus { get; private set; } = "Наименование статуса путевого листа";

        public string UnitNumber { get; private set; } = "Номер единицы оборудования";
        /// <summary>
        /// From first file
        /// </summary>
        public string UnitModel { get; private set; } = "Марка единицы оборудования";
        public string StateNumber { get; private set; } = "Гос.знак единицы оборудования";
        public string NumberOfDriver { get; private set; } = "Табельный номер водителя";
        public string FullNameOfDriver { get; private set; } = "ФИО водителя";
        public string NumberOfCustomer { get; private set; } = "Табельный номер заказчика";
        public string FullNameOfCustomer { get; private set; } = "ФИО заказчика";
               
        public string DepartureFromGarageDate { get; private set; } = "Дата выезда из гаража (факт)";
        public string DepartureFromGarageTime { get; private set; } = "Время выезда из гаража (факт)";
        public string ReturnToGarageDate { get; private set; } = "Дата возвращения в гараж (факт)";
        public string ReturnToGarageTime { get; private set; } = "Время возвращения в гараж (факт)";
        public string TimeOnDutyAtAll { get; private set; } = "Время в наряде (всего)";
               
        public string DepartureOdometerValue { get; private set; } = "Показания одометра (выезд)";
        public string ReturnOdometerValue { get; private set; } = "Показания одометра (возврат)";
        public string TotalMileage { get; private set; } = "Общий пробег";
        public string DepartureMotoHoursIndications { get; private set; } = "Показания моточасов (выезд)";
        public string ReturnMotoHoursIndications { get; private set; } = "Показания моточасов (возврат)";
        public string MotoHoursIndicationsAtAll { get; private set; } = "Моточасов всего";

        public string CountTrips { get; private set; } = "Количество маршрутов в ПЛ";
        public string DriverWorkMode { get; private set; } = "Режим работы водителя";
        public string DriverWorkModeDescription { get; private set; } = "Режим работы водителя - текст";
               
        public string DepartmentCode { get; private set; } = "Служба";
        public string DepartmentTitle { get; private set; } = "Наименование Службы";
        
        #region Fuel
        public string DepartureBalanceGas { get; private set; } = "Бензин Остаток при Выезде 1";
        public string ReturnBalanceGas { get; private set; } = "Бензин Остаток при Возвращении 1";
        public string ConsumptionGasActual { get; private set; } = "Бензин Фактический расход 1";
        public string ConsumptionGasNormative { get; private set; } = "Бензин Нормативный расход 1";
        public string ConsumptionGasSavingsOrOverruns { get; private set; } = "Бензин Экономия/Перерасход 1";
               
        public string DepartureBalanceDisel { get; private set; } = "ДТ Остаток при Выезде 2";
        public string ReturnBalanceDisel { get; private set; } = "ДТ Остаток при Возвращении 2";
        public string ConsumptionDiselActual { get; private set; } = "ДТ Фактический расход 2";
        public string ConsumptionDiselNormative { get; private set; } = "ДТ Нормативный расход 2";
        public string ConsumptionDiselSavingsOrOverruns { get; private set; } = "ДТ Экономия/Перерасход 2";
               
        public string DepartureBalanceLPG { get; private set; } = "Газ Остаток при Выезде 3";
        public string ReturnBalanceLPG { get; private set; } = "Газ Остаток при Возвращении 3";
        public string ConsumptionLPGActual { get; private set; } = "Газ Фактический расход 3";
        public string ConsumptionLPGNormative { get; private set; } = "Газ Нормативный расход 3";
        public string ConsumptionLPGSavingsOrOverruns { get; private set; } = "Газ Экономия/Перерасход 3";
        #endregion Fuel
        #endregion


        #region Headers from second file
        public string Departments { get; private set; } = "Службы/отделы";
        public string TypeOfVehicle { get; private set; } = "Тип";
        public string PartOfStructureNameForResult { get; private set; } = "Структурные подразделени";
        /// <summary>
        /// From second file
        /// </summary>
        public string ModelOfVehicle { get; private set; } = "Марка";
        #endregion


        #region Headers to result first file
        public string TotalMileageResult { get; private set; } = "Общий пробег за период, км";
        public string TotalJobDoneResult { get; private set; } = "Общая наработка мото/час";
        public string ConsumptionGasActualResult { get; private set; } = "Бензин Фактический расход, л";
        public string ConsumptionDieselActualResult { get; private set; } = "ДТ Фактический расход, л.";
        public string ConsumptionLPGActualResult { get; private set; } = "Газ Фактический расход, л";
        #region Cost Headers
        public string Amortization { get; private set; } = "Амортизация";
        public string DriversFOT { get; private set; } = "ФОТ водителя, грн.";
        public string TotalCost { get; private set; } = "Всего затрат, грн.";
        public string TotalCostDisel { get; private set; } = "Затраты на топливо (ДТ), грн без НДС";
        public string TotalCostGas { get; private set; } = "Затраты на топливо (бензин), грн без НДС";
        public string TotalCostLPG { get; private set; } = "Затраты на топливо (ГАЗ), грн без НДС";
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
                      .GetProperties()
                      .Where(f => f.PropertyType == typeof(string))
                      .ToDictionary(f => f.Name,
                                    f => (string)f.GetValue(this));
        }


        public string GetFieldValueByPropName(object obj, string property)
        {
            return (string)obj.GetType()
                      .GetProperty(property)
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
