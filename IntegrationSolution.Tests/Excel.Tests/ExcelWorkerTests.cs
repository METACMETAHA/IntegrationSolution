﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IntegrationSolution.Tests.Excel.Tests
{
    [TestClass]
    public class ExcelWorkerTests
    {
        //[DataTestMethod]
        //public void TestGetCars()
        //{

        //}


        [DataTestMethod]
        [DataRow(@"C:\Users\user\source\repos\IntegrationSolution\IntegrationSolution.Tests\test.txt")]
        public void TestIsExistFile(string PathToFile)
        {
            Assert.IsTrue(File.Exists(PathToFile));
        }


        [DataTestMethod]
        [DataRow(@"C:\Users\user\source\repos\IntegrationSolution\IntegrationSolution.Tests\test.txt")]
        public void TestIsExistData(string PathToFile)
        {
            ICollection<string> _listSurnames = File.ReadAllLines(PathToFile);
            Assert.IsNotNull(_listSurnames);
            Assert.AreNotEqual(_listSurnames.Count, 0);
        }

        [DataTestMethod]
        [DataRow(@"C:\Users\user\source\repos\IntegrationSolution\IntegrationSolution.Tests\test.txt")]
        public void TestFillCheck(string PathToFile)
        {
            ICollection<string> _listSurnames = File.ReadAllLines(PathToFile);
            bool isFillError = false;

            foreach (var line in _listSurnames)
            {
                var dataInLine = line.Split(';');
                if (dataInLine.Length != 2)
                {
                    isFillError = true;
                    break;
                }
            }

            Assert.IsFalse(isFillError);
        }

        [DataTestMethod]
        [DataRow(@"C:\Users\user\source\repos\IntegrationSolution\IntegrationSolution.Tests\test.txt")]
        public void TestDataCheck(string PathToFile)
        {
            ICollection<string> _listSurnames = File.ReadAllLines(PathToFile);
            bool isInDataError = false;

            foreach (var line in _listSurnames)
            {
                var dataInLine = line.Split(';');
                if (dataInLine.Length > 1)
                {
                    if (dataInLine[1].Trim() != "М" && dataInLine[1].Trim() != "Ж")
                    {
                        isInDataError = true;
                        break;
                    }
                }
            }
            Assert.IsFalse(isInDataError);
        }
    }
}
