using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Events;
using IntegrationSolution.Common.ModulesExtension.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace IntegrationSolution.Common.ModulesExtension.Implementations
{
    /// <summary>
    /// Current abstration of each ViewModel is needed for store Title of each node in WizzardControl.
    /// Implement BindableVase and IModuleViewModel
    /// </summary>
    public abstract class ViewModelBase : BindableBase, IModuleViewModelNavigation, IActiveState, INotifyEvents
    {
        protected IUnityContainer _container;
        protected IEventAggregator _eventAggregator;

        #region Properties
        // TODO: in future, name of each node should be loaded from dictionary by nameof class
        public string Title { get; protected set; }


        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }


        private bool _isFinished;
        public bool IsFinished
        {
            get { return _isFinished; }
            protected set { SetProperty(ref _isFinished, value); }
        }


        private bool _canGoNext;
        public bool CanGoNext
        {
            get { return _canGoNext; }
            set
            { SetProperty(ref _canGoNext, value); }
        }


        private bool _canGoBack;
        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { SetProperty(ref _canGoBack, value); }
        }


        private Error _error;
        public Error Error
        {
            get { return _error; }
            set { SetProperty(ref _error, value); }
        }
        #endregion Properties


        public ViewModelBase(IUnityContainer container, IEventAggregator ea)
        {
            _container = container;
            _eventAggregator = ea;

            // TODO: in future, name of each node should be loaded from dictionary by nameof class
            this.Title = this.GetType().Name;

            CanGoBack = false;
            CanGoNext = false;
            IsFinished = false;
        }


        public abstract bool MoveNext();


        public abstract bool MoveBack();


        public void OnEnter()
        {
            IsActive = true;

            _eventAggregator.GetEvent<SubmitFinishedEvent>().Subscribe(UpdateSubmitFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Subscribe(UpdateGoNext);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Subscribe(UpdateStatus);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Publish(Error);
        }


        public void OnExit()
        {
            IsActive = false;

            _eventAggregator.GetEvent<SubmitFinishedEvent>().Unsubscribe(UpdateSubmitFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Unsubscribe(UpdateGoNext);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Unsubscribe(UpdateStatus);
        }


        public void NotifyOnUpdateEvents()
        {
            _eventAggregator.GetEvent<StatusUpdateEvent>().Publish(Error);
            _eventAggregator.GetEvent<SubmitFinishedEvent>().Publish(IsFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Publish(CanGoNext);
        }


        #region EventActions
        protected void UpdateSubmitFinished(bool obj)
        {
            IsFinished = obj;
            // RaisePropertyChanged(nameof(IsFinished));
        }


        protected void UpdateGoNext(bool obj)
        {
            CanGoNext = obj;
            // RaisePropertyChanged(nameof(CanGoNext));
        }


        protected void UpdateStatus(Error obj)
        {
            Error = obj;
            // RaisePropertyChanged(nameof(Error));
        }
        #endregion EventActions
    }
}
