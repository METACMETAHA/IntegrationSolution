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
using System.Windows.Controls;

namespace Integration.PartialViews.ViewModels
{
    public class PredictionChartViewModel : BindableBase
    {
        public SeriesCollection Series { get; set; }

        private Series selectedSeries;
        public Series SelectedSeries
        {
            get { return selectedSeries; }
            set
            {
                SetProperty(ref selectedSeries, value);
                //if (SelectedSeries == null)
                //    return;

                //var ser = Series.FirstOrDefault(x => x.Title == SelectedSeries.Title);

                //if (ser == null)
                //    return;

                //int index = Series.IndexOf(ser);

                //var series = ((StackedAreaSeries)ser.Model.SeriesCollection.ElementAt(index));
                //series.Visibility = series.Visibility == Visibility.Visible
                //    ? Visibility.Hidden
                //    : Visibility.Visible;
            }
        }


        public PredictionChartViewModel()
        {
            OnPreviewMouseDown = new DelegateCommand(OnPreviewMouseDownCmd);

            Series = new SeriesCollection
            {
                new StackedAreaSeries
                {
                    Values = new ChartValues<double> {20, 30, 35, 45, 65, 85},
                    Title = "Electricity"
                },
                new StackedAreaSeries
                {
                    Values = new ChartValues<double> {10, 12, 18, 20, 38, 40},
                    Title = "Water"
                },
                new StackedAreaSeries
                {
                    Values = new ChartValues<double> {5, 8, 12, 15, 22, 25},
                    Title = "Solar"
                },
                new StackedAreaSeries
                {
                    Values = new ChartValues<double> {10, 12, 18, 20, 38, 40},
                    Title = "Gas"
                }
            };
        }


        public DelegateCommand OnPreviewMouseDown { get; private set; }
        protected void OnPreviewMouseDownCmd()
        {
            if (SelectedSeries == null)
                return;

            var ser = Series.FirstOrDefault(x => x.Title == SelectedSeries.Title);

            if (ser == null)
                return;
            
            var series = (StackedAreaSeries)ser;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;

            //var item = ItemsControl.ContainerFromElement(ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            //if (item == null) return;

            //var series = (StackedAreaSeries)item.Content;
            //series.Visibility = series.Visibility == Visibility.Visible
            //    ? Visibility.Hidden
            //    : Visibility.Visible;
        }
    }
}
