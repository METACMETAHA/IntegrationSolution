using Integration.Infrastructure.Constants;
using IntegrationSolution.Common.ModulesExtension.Implementations;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Unity;

namespace Integration.Infrastructure.ViewModels
{
    using Data = KeyValuePair<ViewModelBase, UserControl>;

    public class BodyViewModel : BindableBase
    {
        protected IUnityContainer _container;

        #region Properties
        /// <summary>
        /// This variable is need for fill percentage between each step in Wizard User Control.
        /// </summary>
        public double OneStepPoints { get; private set; }


        /// <summary>
        /// This variable is need for fill percentage at all in Wizard User Control.
        /// </summary>
        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set { SetProperty(ref _progress, value); }
        }


        private ConfigurationData _configData;
        public ConfigurationData ConfigData
        {
            get { return _configData; }
            set { SetProperty(ref _configData, value); }
        }
        #endregion Properties


        public BodyViewModel(IUnityContainer container, IEventAggregator ea)
        {
            _container = container;
            
            ConfigData = container.Resolve<ConfigurationData>();

            MoveCommandPrev = new DelegateCommand(MovePrev);
            MoveCommandNext = new DelegateCommand(MoveNext, CanMoveNext);

            Initialize();
        }


        public void Initialize()
        {
            if (ConfigData.Steps != null)
                Progress += OneStepPoints = (ConfigData.Steps.Count > 0) ? 100 / ConfigData.Steps.Count : 100; // 100 - is Max value of progress
        }


        #region Commands
        public DelegateCommand MoveCommandPrev { get; private set; }
        private void MovePrev()
        {
            var index = ConfigData.Steps.IndexOf(ConfigData.SelectedVM);

            if (ConfigData.SelectedVM.Key.MoveBack())
            {
                ConfigData.SelectedVM = (0 >= index - 1) ? ConfigData.Steps.FirstOrDefault() : ConfigData.Steps.ElementAt(index - 1);
                Progress = (this.Progress > OneStepPoints) ? Progress -= OneStepPoints : OneStepPoints;
            }
        }


        public DelegateCommand MoveCommandNext { get; private set; }
        private void MoveNext()
        {
            var index = ConfigData.Steps.IndexOf(ConfigData.SelectedVM);

            if (ConfigData.SelectedVM.Key.MoveNext().Result)
            {
                ConfigData.SelectedVM = (ConfigData.Steps.Count > index + 1) ? ConfigData.Steps.ElementAt(index + 1) : ConfigData.Steps.LastOrDefault();
                Progress = (this.Progress < 100) ? Progress += OneStepPoints : 100;
            }
        }
        private bool CanMoveNext()
        {
            return true;
        }
        #endregion Commands
    }
}
