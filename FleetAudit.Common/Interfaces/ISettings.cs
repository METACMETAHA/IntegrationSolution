using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetAudit.Common.Interfaces
{
    public interface ISettings
    {
        object this[string propertyName] { get; set; }
    }
}
