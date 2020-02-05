using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IntegrationSolution.Dialogs.Views
{
    /// <summary>
    /// Interaction logic for FuelPriceInputDialog.xaml
    /// </summary>
    public partial class FuelPriceInputDialog : BaseMetroDialog
    {
        public FuelPriceInputDialog()
        {
            InitializeComponent();
        }

        public FuelPriceInputDialog(MetroWindow owningWindow, MetroDialogSettings settings) : base(owningWindow, settings)
        {
            InitializeComponent();
        }
    }
}
