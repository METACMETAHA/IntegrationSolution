using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WialonBase.Configuration;
using WialonBase.Implementation;

namespace IntegrationSolution.Tests.Wialon.Tests
{
    [TestClass]
    public class WialonConnectionTest
    {
        protected IUnityContainer _container;


        public WialonConnectionTest()
        {
        }


        [DataTestMethod]
        public void TestWialonConnection()
        {
            WialonConnection con = new WialonConnection();
            var connection = con.TryConnect();
            var close = con.TryClose();
            Assert.IsTrue(connection);
            Assert.IsTrue(close);
        }


        //[DataTestMethod]
        //public void TestGetVehiclesWialon()
        //{
        //    //WialonOperations con = _container.Resolve<WialonOperations>();
        //    WialonOperations con = new WialonOperations(null);
        //    var connection = con.Open();
        //    var cars = con.GetCarsEnumarable();
        //    var close = con.Close();
        //    Assert.IsTrue(connection);
        //    Assert.IsTrue(close);
        //}
    }
}
