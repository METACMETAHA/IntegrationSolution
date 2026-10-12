using FleetAudit.Common.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Common.Implementations
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
                using (FileStream fs = new FileStream(ResolveFilePath(), FileMode.Open))
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
                string path = ResolveFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                    formatter.Serialize(fs, data);
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.Message);
            }
        }

        /// <summary>
        /// The full path of <see cref="FileName"/>. A relative name lives in <c>%LocalAppData%\FleetAudit</c>, not in the
        /// working directory: the app is installed per machine, so the install folder is read-only for a standard user.
        /// </summary>
        public string ResolveFilePath()
        {
            if (Path.IsPathRooted(FileName))
                return FileName;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FleetAudit",
                FileName);
        }
    }
}
