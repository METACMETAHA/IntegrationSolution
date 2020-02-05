using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToastNotifications.Core;

namespace NotificationConstructor.Interfaces
{
    public interface INotificationManager
    {
        void NotifyInformationAsync(string message, MessageOptions options = null);

        void NotifyErrorAsync(string message, MessageOptions options = null);

        void NotifyWarningAsync(string message, MessageOptions options = null);

        void NotifySuccessAsync(string message, MessageOptions options = null);
    }
}
