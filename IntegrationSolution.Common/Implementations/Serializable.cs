using IntegrationSolution.Common.Interfaces;
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

        public string FileName { get; protected set; }

        public Serializable(string file)
        {
            FileName = file;
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
            { return null; }
            return result ?? null;
        }

        public void Serialize(T data)
        {
            formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(this.FileName, FileMode.OpenOrCreate))
                formatter.Serialize(fs, data);
        }
    }
}
