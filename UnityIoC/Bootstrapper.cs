using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace UnityIoC
{
    public class Bootstrapper
    {
        private static UnityContainer _container;

        public static void Startup()
        {
            _container = new UnityContainer();

            _container.RegisterType<ExcelPackage>();

        }
    }
}
