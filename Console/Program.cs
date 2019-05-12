using IntegrationSolution.Excel.Implementations;
using IntegrationSolution.Excel.Interfaces;
using System;
using System.Windows;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Consoles
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string site = "https://hst-api.wialon.com";
            
            using (var client = new WebClient())
            {
                var values = new NameValueCollection
                {
                    ["svc"] = "token/login",
                    ["params"] = "{\"token\":\"93662d5dd4ed0a21b9775bd4704d6666895DABE9AB194AF87912246CE60488C6F8B4D168\"}"
                };
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                var response = client.UploadValues("http://dtekgps.ohholding.com.ua/wialon/ajax.html", values);

                var responseString = Encoding.Default.GetString(response);
                Console.WriteLine(responseString);

                JObject json = JObject.Parse(responseString);
            }
            Console.WriteLine("End");
        }
    }
}
