using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Integration.ModuleGUI.Views
{
    /// <summary>
    /// Interaction logic for OperationsView.xaml
    /// </summary>
    public partial class OperationsView : UserControl
    {
        public OperationsView()
        {
            InitializeComponent();
            SizeChanged += OperationsView_SizeChanged;
        }

        private void OperationsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth >= 1100)
                MainDriversChart.Width = this.ActualWidth / 2;
            else
                MainDriversChart.Width = this.ActualWidth / 2.8;
            
            if (SapDetaisPanel.ActualHeight >= 450)
            {
                expanderListBox.Height = this.ActualHeight / 1.2;
                MainDriversChart.Height = this.ActualHeight / 1.4;
            }
            else
            {
                expanderListBox.Height = this.ActualHeight / 1.5;
                MainDriversChart.Height = this.ActualHeight / 1.8;
            }
        }
    }
}
