using NotificationConstructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;
using ToastNotifications.Core;
using Unity;
using System.Threading;
using Unity.Attributes;

namespace NotificationConstructor.Implementations
{
    public class NotificationManager : INotificationManager
    {
        //[Dependency]
        //public IUnityContainer container { get; set; }

        private readonly Notifier _notifier;

        public NotificationManager()
        {

            _notifier = new Notifier(config =>
            {
                config.Dispatcher = Application.Current.Dispatcher;

                config.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                config.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(4));
            });
        }

        public void NotifyErrorAsync(string message, MessageOptions options = null)
        {
            if (options == null)
                _notifier.ShowError(message);
            else
                _notifier.ShowError(message, options);
        }

        public void NotifyInformationAsync(string message, MessageOptions options = null)
        {
            if (options == null)
                _notifier.ShowInformation(message);
            else
                _notifier.ShowInformation(message, options);
        }

        public void NotifySuccessAsync(string message, MessageOptions options = null)
        {
            if (options == null)
                _notifier.ShowSuccess(message);
            else
                _notifier.ShowSuccess(message, options);
        }

        public void NotifyWarningAsync(string message, MessageOptions options = null)
        {
            if (options == null)
                _notifier.ShowWarning(message);
            else
                _notifier.ShowWarning(message, options);
        }
    }
}
