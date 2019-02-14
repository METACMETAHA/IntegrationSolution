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
        #region Headers
        public static readonly string PathListType = "Вид путевого листа";
        public static readonly string PathListStatus = "Наименование статуса путевого листа";

        public static readonly string UnitNumber = "Номер единицы оборудования";
        public static readonly string UnitModel = "Марка единицы оборудования";
        public static readonly string StateNumber = "Гос.знак единицы оборудования";
        public static readonly string NumberOfDriver = "Табельный номер водителя";
        public static readonly string FullNameOfDriver = "ФИО водителя";
        public static readonly string NumberOfCustomer = "Табельный номер заказчика";
        public static readonly string FullNameOfCustomer = "ФИО заказчика";
               
        public static readonly string DepartureFromGarageDate = "Дата выезда из гаража (факт)";
        public static readonly string DepartureFromGarageTime = "Время выезда из гаража (факт)";
        public static readonly string ReturnToGarageDate = "Дата возвращения в гараж (факт)";
        public static readonly string ReturnToGarageTime = "Время возвращения в гараж (факт)";
        public static readonly string TimeOnDutyAtAll = "Время в наряде (всего)";
               
        public static readonly string DepartureOdometerValue = "Показания одометра (выезд)";
        public static readonly string ReturnOdometerValue = "Показания одометра (возврат)";
        public static readonly string TotalMileage = "Общий пробег";
        public static readonly string DepartureMotoHoursIndications = "Показания моточасов (выезд)";
        public static readonly string ReturnMotoHoursIndications = "Показания моточасов (возврат)";
        public static readonly string MotoHoursIndicationsAtAll = "Моточасов всего";

        public static readonly string CountTrips = "Количество маршрутов в ПЛ";
        public static readonly string DriverWorkMode = "Режим работы водителя";
        public static readonly string DriverWorkModeDescription = "Режим работы водителя - текст";
               
        public static readonly string DepartmentCode = "Служба";
        public static readonly string DepartmentTitle = "Наименование Службы";
               
        public static readonly string DepartureBalanceGas = "Бензин Остаток при Выезде 1";
        public static readonly string ReturnBalanceGas = "Бензин Остаток при Возвращении 1";
        public static readonly string ConsumptionGasActual = "Бензин Фактический расход 1";
        public static readonly string ConsumptionGasNormative = "Бензин Нормативный расход 1";
        public static readonly string ConsumptionGasSavingsOrOverruns = "Бензин Экономия/Перерасход 1";
               
        public static readonly string DepartureBalanceDisel = "ДТ Остаток при Выезде 2";
        public static readonly string ReturnBalanceDisel = "ДТ Остаток при Возвращении 2";
        public static readonly string ConsumptionDiselActual = "ДТ Фактический расход 2";
        public static readonly string ConsumptionDiselNormative = "ДТ Нормативный расход 2";
        public static readonly string ConsumptionDiselSavingsOrOverruns = "ДТ Экономия/Перерасход 2";
               
        public static readonly string DepartureBalanceLPG = "Газ Остаток при Выезде 3";
        public static readonly string ReturnBalanceLPG = "Газ Остаток при Возвращении 3";
        public static readonly string ConsumptionLPGActual = "Газ Фактический расход 3";
        public static readonly string ConsumptionLPGNormative = "Газ Нормативный расход 3";
        public static readonly string ConsumptionLPGsSavingsOrOverruns = "Газ Экономия/Перерасход 3";
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
