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
    /// <summary>
    /// Talks to the live Wialon server with a personal token from the WIALON_TOKEN environment variable;
    /// inconclusive when it is not set, and excluded from CI ("TestCategory!=Integration").
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class WialonConnectionTest
    {
        protected IUnityContainer _container;
        

        private static string GetToken()
        {
            var token = Environment.GetEnvironmentVariable("WIALON_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
                Assert.Inconclusive("Set the WIALON_TOKEN environment variable to a Wialon access token to run this test.");
            return token;
        }


        [DataTestMethod]
        public void TestWialonConnection()
        {
            WialonConnection con = new WialonConnection();
            var connection = con.TryConnect(GetToken());
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
            var connection = con.TryConnect(GetToken());
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
            var connection = con.TryConnect(GetToken());
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
            var connection = con.TryConnect(GetToken());
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
            var connection = con.TryConnect(GetToken());
            var clean = con.CleanUpResults();
            var close = con.TryClose();
            
            Assert.IsTrue(connection);
            Assert.IsTrue(clean);
            Assert.IsTrue(close);
        }
    }
}
