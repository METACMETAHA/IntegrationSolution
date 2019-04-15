using IntegrationSolution.Common.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Entities;

namespace WialonBase.Helpers
{
    public static class JsonToVehicleConverter
    {
        public static CarWialon ToCarWialon(this JToken obj)
        {
            try
            {
                var id = int.Parse(obj["id"].Value<string>());
                var state = obj["nm"].Value<string>().ToStateNumberWialon();

                return new CarWialon(id, state);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
