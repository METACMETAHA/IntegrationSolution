using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityIoC;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Startup();

            var file = "export.xlsx";
        }
    }
}
