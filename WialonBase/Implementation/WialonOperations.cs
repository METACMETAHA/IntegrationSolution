using IntegrationSolution.Entities.Interfaces;
using log4net;
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
using WialonBase.Helpers;
using WialonBase.Interfaces;

namespace WialonBase.Implementation
{
    public class WialonOperations : INavigationOperations, IDisposable
    {
        private readonly IUnityContainer _container;
        private readonly ILog _logger;
        private WialonConnection _wialonConnection;


        public WialonOperations(IUnityContainer unityContainer)
        {
            _container = unityContainer;
            _logger = LogManager.GetLogger(this.GetType());
            _wialonConnection = _container.Resolve<WialonConnection>();
            Open();
        }


        public bool Open() => _wialonConnection.Connect();

        public bool Close() => _wialonConnection.TryClose();


        public IEnumerable<CarWialon> GetCarsEnumarable()
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


        public void Dispose()
        {
            this.Close();
        }
    }
}
