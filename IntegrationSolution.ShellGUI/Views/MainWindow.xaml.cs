using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace IntegrationSolution.ShellGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(ILocalizationService localization)
        {
            InitializeComponent();

            ApplyDialogTexts();
            LanguageChangedEventManager.AddHandler(localization, OnLanguageChanged);
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            // An exception here would stop the remaining listeners from following the switch.
            try
            {
                ApplyDialogTexts();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to update the dialog button texts for '{e.NewLanguage.CultureName}'. {ex}");
            }
        }

        /// <summary>
        /// Default button texts of MahApps message/input/progress dialogs shown over this window
        /// (they are used whenever a dialog is shown without explicit settings).
        /// </summary>
        private void ApplyDialogTexts()
        {
            MetroDialogOptions.AffirmativeButtonText = Strings.Common_Ok;
            MetroDialogOptions.NegativeButtonText = Strings.Common_Cancel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Dispatcher.InvokeShutdown();
        }
    }
}
