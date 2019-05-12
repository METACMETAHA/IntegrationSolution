using IntegrationSolution.Common.Implementations;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WialonBase.Helpers;
using WialonBase.Interfaces;

namespace WialonBase.Configuration
{
    public class WialonConnection : PropertyChangedBase, IConnection, IDisposable
    {
        #region Variables
        protected readonly ILog _logger;

        private readonly string _contentType = "application/x-www-form-urlencoded";
        private JObject _jsonConnectionInfo;
        #endregion


        #region Properties
        public string Token { get; private set; }
        
        public string APIUrl { get; private set; }

        public string Host => _jsonConnectionInfo["host"].Value<string>();
        public string SessionID => _jsonConnectionInfo["eid"].Value<string>();
        #endregion


        public WialonConnection()
        {
            _logger = LogManager.GetLogger(this.GetType());
            Token = ConfigurationManager.AppSettings[nameof(Token)];
            APIUrl = ConfigurationManager.AppSettings[nameof(APIUrl)];
        }


        public bool TryConnect()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        ["svc"] = "token/login",
                        ["params"] = "{\"token\":\"" + Token + "\"}"
                    };
                    client.Headers[HttpRequestHeader.ContentType] = _contentType;

                    var response = client.UploadValues(APIUrl, values);

                    var responseString = Encoding.Default.GetString(response);
                    _jsonConnectionInfo = JObject.Parse(responseString);

                    var error = CheckError(_jsonConnectionInfo);
                    if (error != null)
                        throw new Exception(error);

                    if(_jsonConnectionInfo["reason"] != null)
                        if (!string.IsNullOrWhiteSpace(_jsonConnectionInfo["reason"].Value<string>()))
                            throw new Exception($"Подключение установлено, но получена следующая ошибка от сервера: {_jsonConnectionInfo["reason"].Value<string>()}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
        }


        public bool TryClose()
        {
            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        ["svc"] = "core/logout",
                        ["params"] = "{}",
                        ["sid"] = SessionID
                    };
                    client.Headers[HttpRequestHeader.ContentType] = _contentType;

                    var response = client.UploadValues(APIUrl, values);

                    var responseString = Encoding.Default.GetString(response);
                    _jsonConnectionInfo = JObject.Parse(responseString);

                    if (_jsonConnectionInfo["error"].Value<string>() != "0")
                        throw new Exception("Can't close Wialon session!");

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return false;
            }
        }
        

        public JObject SendRequest(string path, params KeyValuePair<string,string> [] parameters)
        { 
            try
            {
                JObject obj;
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        ["svc"] = path,
                        ["sid"] = SessionID
                    };

                    StringBuilder param = new StringBuilder();
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i == parameters.Length - 1)
                            param.Append($"\"{parameters[i].Key}\":\"{parameters[i].Value}\"");
                        else
                            param.Append($"\"{parameters[i].Key}\":\"{parameters[i].Value}\",");
                    }
                    values["params"] = "{\"spec\":{\"itemsType\":\"avl_unit\",\"propName\":\"sys_name\",\"propValueMask\":\" * \",\"sortType\":\"sys_name\"},\"force\":1,\"flags\":\"0x00000001\",\"from\":0,\"to\":0}";
                    client.Headers[HttpRequestHeader.ContentType] = _contentType;

                    var response = client.UploadValues(APIUrl, values);

                    var responseString = Encoding.Default.GetString(response);
                    obj = JObject.Parse(responseString);
                }

                return obj;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }


        public JObject SendRequest(string path, string param)
        {
            try
            {
                JObject obj;
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        ["svc"] = path,
                        ["sid"] = SessionID,
                        ["params"] = param
                    };
                    client.Headers[HttpRequestHeader.ContentType] = _contentType;
                    client.Headers[HttpRequestHeader.ContentEncoding] = Encoding.UTF8.BodyName;
                    var response = client.UploadValues(APIUrl, values);

                    var responseString = Encoding.Default.GetString(response);
                    obj = JObject.Parse(responseString);
                }

                return obj;
            }
            //catch (NullReferenceException ex)
            //{
            //    _logger.Info("Обновление сессии.");
            //    return null;
            //}
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                return null;
            }
        }


        public string CheckError(JObject jObject)
        {
            try
            {
                var code = jObject["error"].Value<int>();
                return WialonExceptions.GetErrorMsg(code);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public void Dispose()
        {
            TryClose();
        }
    }
}
