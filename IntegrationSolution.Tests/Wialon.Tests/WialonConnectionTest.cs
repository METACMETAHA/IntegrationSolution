using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using WialonBase.Configuration;
using WialonBase.Implementation;
using WialonBase.Interfaces;

namespace IntegrationSolution.Tests.Wialon.Tests
{
    [TestClass]
    public class WialonConnectionTest
    {
        protected IUnityContainer _container;
        


        [DataTestMethod]
        public void TestWialonConnection()
        {
            WialonConnection con = new WialonConnection();
            var connection = con.TryConnect();
            var close = con.TryClose();
            Assert.IsTrue(connection);
            Assert.IsTrue(close);
        }


        [DataTestMethod]
        public void TestGetCarsEnumarable()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterSingleton<WialonConnection>();

            WialonWrapper con = new WialonWrapper(container);
            var connection = con.TryConnect();
            var cars = con.GetCarsEnumarable();
            var close = con.TryClose();

            Assert.IsTrue(connection);
            Assert.IsNotNull(cars);
            Assert.IsFalse(cars.Count == 0);
            Assert.IsTrue(close);
        }


        [DataTestMethod]
        [DataRow(1023)]
        public void TestGetCarInfo(int ID)
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterSingleton<WialonConnection>();

            WialonWrapper con = new WialonWrapper(container);
            var connection = con.TryConnect();
            var cars = con.GetCarInfo(ID, new DateTime(2019, 5, 3), DateTime.Now);
            var close = con.TryClose();

            Assert.IsTrue(connection);
            Assert.IsNotNull(cars);
            Assert.IsTrue(close);
        }


        [DataTestMethod]
        [DataRow(924)]
        [DataRow(954)]
        public void TestGetCarInfoDetails(int ID)
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterSingleton<WialonConnection>();

            WialonWrapper con = new WialonWrapper(container);
            var connection = con.TryConnect();
            var cars = con.GetCarInfoDetails(ID, new DateTime(2019, 5, 1), DateTime.Now);
            var close = con.TryClose();

            Assert.IsTrue(connection);
            Assert.IsNotNull(cars);
            Assert.IsNotNull(cars.Trips);
            Assert.IsTrue(close);
        }


        [DataTestMethod]
        public void TestClean()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterSingleton<WialonConnection>();
            
            WialonWrapper con = new WialonWrapper(container);
            var connection = con.TryConnect();
            var clean = con.CleanUpResults();
            var close = con.TryClose();
            
            Assert.IsTrue(connection);
            Assert.IsTrue(clean);
            Assert.IsTrue(close);
        }
    }
}
