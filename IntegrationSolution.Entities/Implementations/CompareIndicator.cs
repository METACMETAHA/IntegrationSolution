using IntegrationSolution.Entities.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Entities.Implementations
{
    public class CompareIndicator<T> : ICommonCompareIndicator<T>
    {
        protected readonly ILog _logger;

        public T SAP { get; set; }
        public T Wialon { get; set; }

        public T Difference {
            get {
                try
                {
                    return (dynamic)SAP - (dynamic)Wialon;
                }
                catch (global::System.Exception)
                {
                    _logger.Error("Невозможно рассчитать разницу. Тип данных: " + typeof(T));
                    return default(T);
                }
            }
        }

        public CompareIndicator()
        {
            _logger = LogManager.GetLogger(this.GetType());
        }
    }
}
