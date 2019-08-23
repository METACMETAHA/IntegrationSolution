using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace IntegrationSolution.Common.ModulesExtension.Implementations
{
    public class ChartsVMBase : BindableBase
    {
        private SeriesCollection series;
        public SeriesCollection Series
        {
            get { return series; }
            set { SetProperty(ref series, value); }
        }

        private Series selectedSeries;
        public Series SelectedSeries
        {
            get { return selectedSeries; }
            set { SetProperty(ref selectedSeries, value); }
        }

        public bool InitializeLocalSeriesData(Dictionary<string, List<double>> data)
        {
            try
            {
                var series = TryCreateSeriesCollection(data);
                if (series != null && series.Any())
                    Series = series;
                else
                    throw new Exception();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        
        public SeriesCollection TryCreateSeriesCollection(Dictionary<string, List<double>> data)
        {
            if (data == null)
                return null;

            SeriesCollection series = new SeriesCollection();
            foreach (var item in data)
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    series.Add(new LineSeries
                    {
                        Values = new ChartValues<double>(item.Value),
                        Title = item.Key,
                        PointForeground = Brushes.White,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 9
                    });
                });
                
            }
            return series;
        }
    }
}
