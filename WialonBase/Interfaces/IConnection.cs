using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WialonBase.Interfaces
{
    public interface IConnection
    {
        void UpdateToken();

        bool TryConnect();

        bool TryConnect(string token);
        
        bool TryClose();

        /// <summary>
        /// This function checks Json parsed response from server for any error.
        /// </summary>
        /// <param name="jObject">parsed response</param>
        /// <returns>Null if response without error or description of error</returns>
        string CheckError(JObject jObject);
    }
}
