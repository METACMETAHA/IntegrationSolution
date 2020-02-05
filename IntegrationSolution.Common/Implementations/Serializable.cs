using IntegrationSolution.Common.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Implementations
{
    [Serializable]
    public class Serializable<T> : ISerializable<T> where T : class
    {
        [NonSerialized]
        protected BinaryFormatter formatter;

        [NonSerialized]
        protected readonly ILog _logger;

        public string FileName { get; protected set; }

        public Serializable(string file)
        {
            FileName = file;
            _logger = LogManager.GetLogger(this.GetType());
        }

        public T Deserialize()
        {
            formatter = new BinaryFormatter();
            T result;

            try
            {
                using (FileStream fs = new FileStream(this.FileName, FileMode.Open))
                    result = (T)formatter.Deserialize(fs);
            }
            catch (Exception)
            {
                return null;
            }
            return result ?? null;
        }

        public void Serialize(T data)
        {
            if (data == null)
                return;

            formatter = new BinaryFormatter();
            try
            {
                using (FileStream fs = new FileStream(this.FileName, FileMode.OpenOrCreate))
                    formatter.Serialize(fs, data);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.Message);
            }
        }
    }
}
