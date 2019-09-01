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
    /// Interaction logic for OperationResultsView.xaml
    /// </summary>
    public partial class OperationResultsView : UserControl
    {
        public OperationResultsView()
        {
            InitializeComponent();
            this.SizeChanged += OperationResultsView_SizeChanged;
        }

        private void OperationResultsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ParentGridTabItem.Height = userControl.ActualHeight / 1.12;
            panelDetails.Height = userControl.ActualHeight / 1.12;
            //ViolationsWnd.Height = ParentGridTabItem.Height - 10;
        }
    }
}
