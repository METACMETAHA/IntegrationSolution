using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Events;
using IntegrationSolution.Common.Interfaces;
using IntegrationSolution.Common.ModulesExtension.Interfaces;
using IntegrationSolution.Localization;
using log4net;
using NotificationConstructor.Interfaces;
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
    public abstract class ViewModelBase : BindableBase, IModuleViewModelNavigation, IActiveState, INotifyEvents, IWindowActions
    {
        protected readonly IUnityContainer _container;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly ILog _logger;
        protected readonly INotificationManager _notificationManager;
        protected readonly ILocalizationService _localization;

        private string _titleResourceKey;

        #region Properties
        private string _title;
        /// <summary>
        /// Title of the node in WizzardControl. Use <see cref="SetTitleResourceKey"/> to make it localizable.
        /// </summary>
        public string Title
        {
            get { return _title; }
            protected set { SetProperty(ref _title, value); }
        }


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
            _logger = LogManager.GetLogger(this.GetType());
            _notificationManager = _container.Resolve<INotificationManager>();
            _localization = _container.Resolve<ILocalizationService>();
            LanguageChangedEventManager.AddHandler(_localization, HandleLanguageChanged);

            this.Title = this.GetType().Name;

            CanGoBack = false;
            CanGoNext = false;
            IsFinished = false;
        }


        /// <summary>
        /// Sets <see cref="Title"/> from a localization resource key; the title follows UI language switching.
        /// </summary>
        protected void SetTitleResourceKey(string resourceKey)
        {
            _titleResourceKey = resourceKey;
            Title = _localization.GetString(resourceKey);
        }


        /// <summary>
        /// Called on the UI thread after the UI language was switched.
        /// Override to refresh texts that are produced in code (XAML texts refresh automatically).
        /// </summary>
        protected virtual void OnLanguageChanged()
        { }


        private void HandleLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            // An exception here would stop the remaining listeners from following the switch.
            try
            {
                if (_titleResourceKey != null)
                    Title = _localization.GetString(_titleResourceKey);

                OnLanguageChanged();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to apply the UI language '{e.NewLanguage.CultureName}'.", ex);
            }
        }


        public abstract Task<bool> MoveNext();


        public abstract bool MoveBack();


        public virtual void OnEnter()
        {
            IsActive = true;

            _eventAggregator.GetEvent<SubmitFinishedEvent>().Subscribe(UpdateSubmitFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Subscribe(UpdateGoNext);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Subscribe(UpdateStatus);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Publish(Error);
        }


        public virtual void OnExit()
        {
            IsActive = false;

            _eventAggregator.GetEvent<SubmitFinishedEvent>().Unsubscribe(UpdateSubmitFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Unsubscribe(UpdateGoNext);
            _eventAggregator.GetEvent<StatusUpdateEvent>().Unsubscribe(UpdateStatus);
        }


        public virtual void NotifyOnUpdateEvents()
        {
            _eventAggregator.GetEvent<StatusUpdateEvent>().Publish(Error);
            _eventAggregator.GetEvent<SubmitFinishedEvent>().Publish(IsFinished);
            _eventAggregator.GetEvent<CanGoNextUpdateEvent>().Publish(CanGoNext);
        }


        public abstract void MaximizeWindow();

        public abstract void NormalizeWindow();


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
