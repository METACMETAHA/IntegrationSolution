using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace IntegrationSolution.Common.Helpers
{
    public class FilteredListView : ListView
    {
        static FilteredListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilteredListView),
                        new FrameworkPropertyMetadata(typeof(FilteredListView)));
        }

        public Func<object, string, bool> FilterPredicate
        {
            get { return (Func<object, string, bool>)GetValue(FilterPredicateProperty); }
            set { SetValue(FilterPredicateProperty, value); }
        }
        public static readonly DependencyProperty FilterPredicateProperty =
            DependencyProperty.Register("FilterPredicate",
            typeof(Func<object, string, bool>), typeof(FilteredListView), new PropertyMetadata(null));

        public Subject<bool> FilterInputSubject = new Subject<bool>();

        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }
        public static readonly DependencyProperty FilterTextProperty =
            DependencyProperty.Register("FilterText",
                typeof(string),
                typeof(FilteredListView),
                new PropertyMetadata("",
                    //This is the 'PropertyChanged' callback that's called 
                    //whenever the Filter input text is changed
                    (d, e) => (d as FilteredListView).FilterInputSubject.OnNext(true)));

        public FilteredListView()
        {
            SetDefaultFilterPredicate();
            InitThrottle();
        }

        private void SetDefaultFilterPredicate()
        {
            FilterPredicate = (obj, text) => obj.ToString().ToLower().Contains(text);
        }

        private void InitThrottle()
        {
            FilterInputSubject.Throttle(TimeSpan.FromMilliseconds(500))
                .ObserveOnDispatcher()
                .Subscribe(HandleFilterThrottle);
        }

        private void HandleFilterThrottle(bool b)
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.ItemsSource);
            if (collectionView == null) return;
            collectionView.Filter = (item) => FilterPredicate(item, FilterText);
        }
    }
}
