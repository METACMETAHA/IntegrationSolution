using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationSolution.Tests.Helpers.Tests
{
    [TestClass]
    public class Converter
    {
        [DataTestMethod]
        [DataRow("АА 0258 ХІ")]
        [DataRow("АА 6548 ОР")]
        [DataRow("")]
        [DataRow("АА 5985 ЕН")]
        [DataRow("19602АІ")]
        [DataRow("АА 6044 ОА")]
        [DataRow("АА 1661 АК")]
        public void TestToStateNumber(string state_number)
        {
            //string testString;
            //testString = ToStateNumber(state_number);
            //Assert.IsNotNull(testString);
        }
    }
}
