using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WialonBase.Interfaces
{
    public interface IConnection
    {
        bool TryConnect();

        bool TryClose();
    }
}
