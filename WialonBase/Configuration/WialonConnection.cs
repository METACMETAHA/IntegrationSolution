using IntegrationSolution.Common.Implementations;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NotificationConstructor.Implementations;
using NotificationConstructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        public string TokenUrl { get; private set; }
        public string OAuth { get; private set; }

        public string Host => _jsonConnectionInfo["host"].Value<string>();
        public string SessionID => _jsonConnectionInfo["eid"].Value<string>();
        #endregion


        public WialonConnection()
        {
            _logger = LogManager.GetLogger(this.GetType());
            Token = ConfigurationManager.AppSettings[nameof(Token)];
            APIUrl = ConfigurationManager.AppSettings[nameof(APIUrl)];
            TokenUrl = ConfigurationManager.AppSettings[nameof(TokenUrl)];
            OAuth = ConfigurationManager.AppSettings[nameof(OAuth)];
        }


        public bool TryConnect()
        {
            return TryConnect(Token);
        }


        public bool TryConnect(string token)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection
                    {
                        ["svc"] = "token/login",
                        ["params"] = "{\"token\":\"" + token + "\"}"
                    };
                    client.Headers[HttpRequestHeader.ContentType] = _contentType;

                    var response = client.UploadValues(APIUrl, values);

                    var responseString = Encoding.Default.GetString(response);
                    _jsonConnectionInfo = JObject.Parse(responseString);

                    var error = CheckError(_jsonConnectionInfo);
                    if (!string.IsNullOrWhiteSpace(error))
                        throw new Exception(error);

                    if (_jsonConnectionInfo["reason"] != null)
                        if (!string.IsNullOrWhiteSpace(_jsonConnectionInfo["reason"].Value<string>()))
                            throw new Exception($"Подключение установлено, но получена следующая ошибка от сервера: {_jsonConnectionInfo["reason"].Value<string>()}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw ex;
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
                throw ex;
            }
        }
        

        public JObject SendRequest(string path, params KeyValuePair<string,string> [] parameters)
        {
            string responseString = null;
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

                    responseString = Encoding.Default.GetString(response);
                    obj = this.ConvertToUTF8AndParseJObject(responseString);
                }

                return obj;
            }
            catch (JsonReaderException ex)
            {
                var arr = ConvertToUTF8AndParseArray(responseString);
                if (arr == null)
                {
                    _logger.Error($"{ex.Message}\nCan`t parse. Method: SendRequest({path})");
                    return null;
                }
                JObject obj = new JObject();
                for (int i = 0; i < arr.Count; i++)
                    obj.Add(i.ToString(), arr[i]);

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
            string responseString = null;
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
                    var response = client.UploadValues(APIUrl, values);

                    responseString = Encoding.Default.GetString(response);                    
                    obj = this.ConvertToUTF8AndParseJObject(responseString);
                }

                return obj;
            }
            catch (JsonReaderException ex)
            {
                var arr = ConvertToUTF8AndParseArray(responseString);
                if (arr == null)
                {
                    _logger.Error($"{ex.Message}\nCan`t parse. Method: SendRequest({path}, {param})");
                    return null;
                }
                JObject obj = new JObject();
                for (int i = 0; i < arr.Count; i++)
                    obj.Add(i.ToString(), arr[i]);

                return obj;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}\nMethod: SendRequest({path}, {param})");
                return null;
            }
        }


        private JObject ConvertToUTF8AndParseJObject(string obj)
        {
            byte[] bytes = Encoding.Default.GetBytes(obj.ToString());
            var encodedString = Encoding.UTF8.GetString(bytes);
            return JObject.Parse(encodedString);
        }

        private JArray ConvertToUTF8AndParseArray(string obj)
        {
            byte[] bytes = Encoding.Default.GetBytes(obj.ToString());
            var encodedString = Encoding.UTF8.GetString(bytes);
            return JArray.Parse(encodedString);
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


        public void UpdateToken()
        {
            Token = ConfigurationManager.AppSettings[nameof(Token)];
        }
    }
}
