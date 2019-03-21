using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel
{
    public class HeaderNames
    {
        #region Headers from first file
        public const string PathListType = "Вид путевого листа";
        public const string PathListStatus = "Наименование статуса путевого листа";

        public const string UnitNumber = "Номер единицы оборудования";
        /// <summary>
        /// From first file
        /// </summary>
        public const string UnitModel = "Марка единицы оборудования";
        public const string StateNumber = "Гос.знак единицы оборудования";
        public const string NumberOfDriver = "Табельный номер водителя";
        public const string FullNameOfDriver = "ФИО водителя";
        public const string NumberOfCustomer = "Табельный номер заказчика";
        public const string FullNameOfCustomer = "ФИО заказчика";
               
        public const string DepartureFromGarageDate = "Дата выезда из гаража (факт)";
        public const string DepartureFromGarageTime = "Время выезда из гаража (факт)";
        public const string ReturnToGarageDate = "Дата возвращения в гараж (факт)";
        public const string ReturnToGarageTime = "Время возвращения в гараж (факт)";
        public const string TimeOnDutyAtAll = "Время в наряде (всего)";
               
        public const string DepartureOdometerValue = "Показания одометра (выезд)";
        public const string ReturnOdometerValue = "Показания одометра (возврат)";
        public const string TotalMileage = "Общий пробег";
        public const string DepartureMotoHoursIndications = "Показания моточасов (выезд)";
        public const string ReturnMotoHoursIndications = "Показания моточасов (возврат)";
        public const string MotoHoursIndicationsAtAll = "Моточасов всего";

        public const string CountTrips = "Количество маршрутов в ПЛ";
        public const string DriverWorkMode = "Режим работы водителя";
        public const string DriverWorkModeDescription = "Режим работы водителя - текст";
               
        public const string DepartmentCode = "Служба";
        public const string DepartmentTitle = "Наименование Службы";
               
        public const string DepartureBalanceGas = "Бензин Остаток при Выезде 1";
        public const string ReturnBalanceGas = "Бензин Остаток при Возвращении 1";
        public const string ConsumptionGasActual = "Бензин Фактический расход 1";
        public const string ConsumptionGasNormative = "Бензин Нормативный расход 1";
        public const string ConsumptionGasSavingsOrOverruns = "Бензин Экономия/Перерасход 1";
               
        public const string DepartureBalanceDisel = "ДТ Остаток при Выезде 2";
        public const string ReturnBalanceDisel = "ДТ Остаток при Возвращении 2";
        public const string ConsumptionDiselActual = "ДТ Фактический расход 2";
        public const string ConsumptionDiselNormative = "ДТ Нормативный расход 2";
        public const string ConsumptionDiselSavingsOrOverruns = "ДТ Экономия/Перерасход 2";
               
        public const string DepartureBalanceLPG = "Газ Остаток при Выезде 3";
        public const string ReturnBalanceLPG = "Газ Остаток при Возвращении 3";
        public const string ConsumptionLPGActual = "Газ Фактический расход 3";
        public const string ConsumptionLPGNormative = "Газ Нормативный расход 3";
        public const string ConsumptionLPGsSavingsOrOverruns = "Газ Экономия/Перерасход 3";
        #endregion


        #region Headers from second file
        public const string Departments = "Службы/отделы";
        public const string TypeOfVehicle = "Тип";
        /// <summary>
        /// From second file
        /// </summary>
        public const string ModelOfVehicle = "Марка";
        #endregion


        #region Headers to result first file
        public const string TotalMileageResult = "Общий пробег за период, км";
        public const string TotalJobDoneResult = "Общая наработка мото/час";
        public const string ConsumptionGasActualResult = "Бензин Фактический расход, л";
        public const string ConsumptionDieselActualResult = "ДТ Фактический расход, л.";
        public const string ConsumptionLPGActualResult = "Газ Фактический расход, л";
        #endregion


        public static Dictionary<string, string> PropertiesData { get; set; }


        static HeaderNames()
        {
            PropertiesData = GetFieldValues(new HeaderNames());
        }


        public static Dictionary<string, string> GetFieldValues(object obj)
        {
            return obj.GetType()
                      .GetFields(BindingFlags.Public | BindingFlags.Static)
                      .Where(f => f.FieldType == typeof(string))
                      .ToDictionary(f => f.Name,
                                    f => (string)f.GetValue(null));
        }
    }
}
