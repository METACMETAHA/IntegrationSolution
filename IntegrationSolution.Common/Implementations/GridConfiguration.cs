using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Implementations
{
    public class GridConfiguration : BindableBase
    {
        private bool _verticalGridLinesIsVisible;
        public bool VerticalGridLinesIsVisible
        {
            get { return _verticalGridLinesIsVisible; }
            set { SetProperty(ref _verticalGridLinesIsVisible, value); }
        }

        private bool _headerIsVisible;
        public bool HeaderIsVisible
        {
            get { return _headerIsVisible; }
            set { SetProperty(ref _headerIsVisible, value); }
        }

        public GridConfiguration()
        {
            HeaderIsVisible = true;
        }
    }
}
