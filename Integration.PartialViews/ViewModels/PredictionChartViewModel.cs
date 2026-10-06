using IntegrationSolution.Common.Models;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using IntegrationSolution.Localization;
using IntegrationSolution.Localization.Resources;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Integration.PartialViews.ViewModels
{
    public class PredictionChartViewModel : ChartsVMBase
    {
        private string title;
        /// <summary>
        /// Chart title. Assigning a text directly detaches the title from <see cref="TitleResourceKey"/>,
        /// so it no longer follows UI language switching; use <see cref="TitleResourceKey"/> for that.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                _titleResourceKey = null;
                SetProperty(ref title, value);
            }
        }

        private string _titleResourceKey;
        /// <summary>
        /// Resource key of the chart title. While it is set, the title is re-read from the resources
        /// every time the UI language changes.
        /// </summary>
        public string TitleResourceKey
        {
            get { return _titleResourceKey; }
            set
            {
                _titleResourceKey = value;
                RefreshTitle();
            }
        }


        public PredictionChartViewModel()
        {
            TitleResourceKey = nameof(Strings.PredictionChart_Title);
            OnPreviewMouseDown = new DelegateCommand(OnPreviewMouseDownCmd);

            // The view model is created on background threads (Task.Run) by its owners. Subscribe on the UI thread,
            // so the weak event manager (and its cleanup) lives there instead of on a thread-pool thread.
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                SubscribeToLanguageChanges();
            else
                dispatcher.BeginInvoke(new Action(SubscribeToLanguageChanges));
        }

        public PredictionChartViewModel(Dictionary<string, List<DateTimePoint>> data) : this()
        {
            if (!InitializeLocalSeriesData(data))
                Series = new SeriesCollection();
        }

        private void SubscribeToLanguageChanges()
        {
            // Weak subscription with a method group (a lambda would be collected); the view keeps this view model alive.
            LanguageChangedEventManager.AddHandler(LocalizationService.Instance, OnLanguageChanged);
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            RefreshTitle();
        }

        private void RefreshTitle()
        {
            if (_titleResourceKey != null)
                SetProperty(ref title, LocalizationService.Instance.GetString(_titleResourceKey), nameof(Title));
        }

        public DelegateCommand OnPreviewMouseDown { get; private set; }
        protected void OnPreviewMouseDownCmd()
        {
            if (SelectedSeries == null)
                return;

            var ser = Series.FirstOrDefault(x => x.Title == SelectedSeries.Title);

            if (ser == null)
                return;

            var series = (LineSeries)ser;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;

            SelectedSeries = null;
        }
    }
}
