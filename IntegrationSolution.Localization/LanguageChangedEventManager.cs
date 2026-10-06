using System;
using System.Windows;

namespace IntegrationSolution.Localization
{
    /// <summary>
    /// Weak event manager for <see cref="ILocalizationService.LanguageChanged"/>.
    /// The localization service lives for the whole application, so subscribers (view models, views)
    /// should listen weakly to avoid being kept alive by it.
    /// </summary>
    public sealed class LanguageChangedEventManager : WeakEventManager
    {
        private LanguageChangedEventManager()
        { }

        public static void AddHandler(ILocalizationService source, EventHandler<LanguageChangedEventArgs> handler)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        public static void RemoveHandler(ILocalizationService source, EventHandler<LanguageChangedEventArgs> handler)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        private static LanguageChangedEventManager CurrentManager
        {
            get
            {
                var managerType = typeof(LanguageChangedEventManager);
                var manager = (LanguageChangedEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new LanguageChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        protected override ListenerList NewListenerList()
        {
            return new ListenerList<LanguageChangedEventArgs>();
        }

        protected override void StartListening(object source)
        {
            ((ILocalizationService)source).LanguageChanged += OnLanguageChanged;
        }

        protected override void StopListening(object source)
        {
            ((ILocalizationService)source).LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            DeliverEvent(sender, e);
        }
    }
}
