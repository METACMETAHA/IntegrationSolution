using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FleetAudit.Settings.Interfaces
{
    public interface IFlyoutViewModel 
    {
        string Header { get; }
        bool IsOpen { get; set; }
        bool IsModal { get; set; }
        bool IsEnabled { get; set; }
        Position Position { get; set; }
        Theme Theme { get; set; }
        Visibility CloseButtonVisibility { get; set; }
    }
}
