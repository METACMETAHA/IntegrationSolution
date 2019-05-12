using IntegrationSolution.Entities.Interfaces;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WialonBase.Configuration;
using WialonBase.Entities;
using WialonBase.Entities.Interfaces;
using WialonBase.Helpers;
using WialonBase.Interfaces;

namespace WialonBase.Implementation
{
    public class WialonWrapper : INavigationOperations, IDisposable
    {
        private readonly IUnityContainer _container;
        private readonly ILog _logger;
        private WialonConnection _wialonConnection;


        public WialonWrapper(IUnityContainer unityContainer)
        {
            _container = unityContainer;
            _logger = LogManager.GetLogger(this.GetType());
            _wialonConnection = _container.Resolve<WialonConnection>();
        }        


        public ICollection<CarWialon> GetCarsEnumarable()
        {
            try
            {
                var result = this._wialonConnection.SendRequest("core/search_items",
                    "{\"spec\":{\"itemsType\":\"avl_unit\",\"propName\":\"sys_name\",\"propValueMask\":\"*\",\"sortType\":\"sys_name\"},\"force\":1,\"flags\":\"0x00000001\",\"from\":0,\"to\":0}");

                if (result == null)
                    throw new Exception("Результаты запроса GetCarsEnumarable() вернули {null}");

                List<CarWialon> carResult = new List<CarWialon>();
                var cars = result["items"].Children();
                foreach (var item in cars)
                {
                    var car = item.ToCarWialon();
                    if(car != null)
                        carResult.Add(car);
                }

                return carResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }


        public TripWialon GetCarInfo(int ID, DateTime from, DateTime to)
        {
            try
            {
                if (!this.CleanUpResults())
                    throw new Exception("Can`t clear results");

                #region Execute exec_report
                var execReport = _wialonConnection.SendRequest("report/exec_report",
                    "{\"reportResourceId\":894,\"reportTemplateId\":1,\"reportTemplate\":null,\"reportObjectId\":" + ID + ",\"reportObjectSecId\":0,\"interval\":{\"flags\":16777216,\"from\":" + from.ToUnixTime() + ",\"to\":" + to.ToUnixTime() + "},\"remoteExec\":1}");

                if (execReport["remoteExec"].Value<int>() != 1)
                    throw new Exception($"Report (Id=894) is failed. Car id={ID}");
                #endregion

                #region Waiting for report
                JObject waitReport = null;
                do {
                    waitReport = _wialonConnection.SendRequest("report/get_report_status", "{}");
                } while (waitReport["status"].Value<int>() == 2);
                #endregion

                if (waitReport["status"].Value<int>() != 4)
                    throw new Exception("Ошибка при ожидании отчёта");

                #region Apply report result
                var applyReport = _wialonConnection.SendRequest("report/apply_report_result", "{}");

                var MainTrip = applyReport.ToTripWialon();
                #endregion

                return MainTrip;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }


        public bool CleanUpResults()
        {
            try
            {
                var result = this._wialonConnection.SendRequest("report/cleanup_result", "{}");

                if (result == null)
                    throw new Exception("Результаты запроса GetCarsEnumarable() вернули {null}");

                if (result["error"].Value<int>() == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
        }


        public void Dispose()
        {
            this.TryClose();
        }


        public bool TryConnect() => _wialonConnection.TryConnect();


        public bool TryClose() => _wialonConnection.TryClose();


        public string CheckError(JObject jObject) => _wialonConnection.CheckError(jObject);
    }
}
