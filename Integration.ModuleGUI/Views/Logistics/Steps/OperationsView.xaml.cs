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
            //listBoxCars.Height = parentDriversGrid.ActualHeight/1.52;
            //panel.Width = this.Width / 3;
        }
    }
}
