using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Interfaces
{
    public interface ISerializable<T> where T : class
    {
        string FileName { get; }

        void Serialize(T data);

        T Deserialize();
    }
}
