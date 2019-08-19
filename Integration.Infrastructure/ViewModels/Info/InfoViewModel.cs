using IntegrationSolution.Common.Helpers;
using NotificationConstructor.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace Integration.Infrastructure.ViewModels.Info
{
    public class InfoViewModel : BindableBase
    {
        protected readonly IUnityContainer _container;
        protected readonly INotificationManager _notificationManager;

        public InfoViewModel(
            IUnityContainer container,
            INotificationManager notificationManager)
        {
            _container = container;
            _notificationManager = notificationManager;

            DownloadCommand = new DelegateCommand<object>(DownloadCmd);
        }


        public DelegateCommand<object> DownloadCommand { get; private set; }
        protected void DownloadCmd(object index)
        {
            try
            {
                int ind = int.Parse(index.ToString());
                FileInfo pathToFile = null;

                #region Get Executable assembley
                var dirs = Directory.GetDirectories(Directory.GetCurrentDirectory());
                DirectoryInfo searchDir = null;
                foreach (var dirName in dirs)
                {
                    var dir = new DirectoryInfo(dirName);
                    if (dir.Name == "Files")
                        searchDir = dir;
                }

                if (searchDir == null)
                    throw new Exception("Ошибка! Файл не найден.");
                #endregion

                var downloadsDir = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Downloads");

                switch (ind)
                {
                    case 1:
                        pathToFile = searchDir.GetFiles("Руководство пользователя.pdf").First();
                        break;

                    case 2:
                        pathToFile = searchDir.GetFiles("Пример заголовков путевых листов.XLSX").First();
                        break;

                    default:
                        throw new Exception("Обратитесь в службу поддержки.");
                }

                if (!File.Exists(pathToFile.FullName))
                    throw new Exception("Файл отсутствует! Обратитесь в службу поддержки.");

                if (Directory.Exists(downloadsDir))
                {
                    File.Delete(Path.Combine(downloadsDir, pathToFile.Name));
                }

                var newFile = pathToFile.CopyTo(Path.Combine(downloadsDir, pathToFile.Name));
                string argument = "/select, \"" + newFile.FullName + "\"";
                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                _notificationManager?.NotifyErrorAsync(ex.Message);
            }
        }
    }
}
