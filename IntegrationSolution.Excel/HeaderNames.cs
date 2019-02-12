using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel
{
    public class HeaderNames
    {
        #region Headers
        public readonly string StateNumber = "Гос.знак единицы оборудования";
        public readonly string PathListType = "Вид путевого листа";
        public readonly string PathListStatus = "Наименование статуса путевого листа";

        public readonly string UnitMark = "Марка единицы оборудования";
        public readonly string NumberOfDriver = "Табельный номер водителя";
        public readonly string FullNameOfDriver = "ФИО водителя";
        public readonly string NumberOfCustomer = "Табельный номер заказчика";
        public readonly string FullNameOfCustomer = "ФИО заказчика";

        public readonly string DepartureFromGarageDate = "Дата выезда из гаража (факт)";
        public readonly string DepartureFromGarageTime = "Время выезда из гаража (факт)";
        public readonly string ReturnToGarageDate = "Дата возвращения в гараж (факт)";
        public readonly string ReturnToGarageTime = "Время возвращения в гараж (факт)";
        public readonly string TimeOnDutyAtAll = "Время в наряде (всего)";

        public readonly string DepartureOdometerValue = "Показания одометра (выезд)";
        public readonly string ReturnOdometerValue = "Показания одометра (возврат)";
        public readonly string TotalMileage = "Общий пробег";
        public readonly string DepartureMotoHoursIndications = "Показания моточасов (выезд)";
        public readonly string ReturnMotoHoursIndications = "Показания моточасов (возврат)";
        public readonly string MotoHoursIndicationsAtAll = "Моточасов всего";

        public readonly string DriverWorkMode = "Режим работы водителя";
        public readonly string DriverWorkModeDescription = "Режим работы водителя - текст";

        public readonly string DepartmentCode = "Служба";
        public readonly string DepartmentTitle = "Наименование Службы";

        public readonly string DepartureBalanceGas = "Бензин Остаток при Выезде 1";
        public readonly string ReturnBalanceGas = "Бензин Остаток при Возвращении 1";
        public readonly string ConsumptionGasActual = "Бензин Фактический расход 1";
        public readonly string ConsumptionGasNormative = "Бензин Нормативный расход 1";
        public readonly string ConsumptionGasSavingsOrOverruns = "Бензин Экономия/Перерасход 1";

        public readonly string DepartureBalanceDisel = "ДТ Остаток при Выезде 2";
        public readonly string ReturnBalanceDisel = "ДТ Остаток при Возвращении 2";
        public readonly string ConsumptionDiselActual = "ДТ Фактический расход 2";
        public readonly string ConsumptionDiselNormative = "ДТ Нормативный расход 2";
        public readonly string ConsumptionDiselSavingsOrOverruns = "ДТ Экономия/Перерасход 2";
        
        public readonly string DepartureBalanceLPG = "Газ Остаток при Выезде 3";
        public readonly string ReturnBalanceLPG = "Газ Остаток при Возвращении 3";
        public readonly string ConsumptionLPGActual = "Газ Фактический расход 3";
        public readonly string ConsumptionLPGNormative = "Газ Нормативный расход 3";
        public readonly string ConsumptionLPGsSavingsOrOverruns = "Газ Экономия/Перерасход 3";
        #endregion
    }
}
