using IntegrationSolution.Common.Models;
using IntegrationSolution.Common.ModulesExtension.Implementations;
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
        public PredictionChartViewModel()
        {
            OnPreviewMouseDown = new DelegateCommand(OnPreviewMouseDownCmd);
        }

        public PredictionChartViewModel(Dictionary<string, List<DateTimePoint>> data) : this()
        {
            if (!InitializeLocalSeriesData(data))
                Series = new SeriesCollection();
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
