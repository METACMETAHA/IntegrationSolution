﻿using IntegrationSolution.Common.Entities;
using IntegrationSolution.Common.Implementations;
using IntegrationSolution.Excel;
using MahApps.Metro.Controls;
using NotificationConstructor.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Integration.PartialViews.ViewModels
{
    public class ChangeHeadersViewModel : BindableBase
    {
        protected readonly SerializeConfigDTO _settings;
        protected readonly INotificationManager _notificationManager;
        protected readonly HeaderNames _headerNames;

        #region Properties
        private ObservableConcurrentDictionary<string, string> headers;
        public ObservableConcurrentDictionary<string, string> Headers
        {
            get { return headers; }
            set { SetProperty(ref headers, value); }
        }

        private KeyValuePair<string, string> selectedHeader;
        public KeyValuePair<string, string> SelectedHeader
        {
            get { return selectedHeader; }
            set
            {
                SetProperty(ref selectedHeader, value);
                ValueForSelectedHeader = value.Value;
            }
        }

        private string valueForSelectedHeader;
        public string ValueForSelectedHeader
        {
            get { return valueForSelectedHeader; }
            set { SetProperty(ref valueForSelectedHeader, value); }
        }
        #endregion

        public ChangeHeadersViewModel(
            SerializeConfigDTO settings,
            INotificationManager notificationManager,
            HeaderNames headerNames)
        {
            _settings = settings;
            _notificationManager = notificationManager;
            _headerNames = headerNames;
            
            UpdateSelectedHeaderCommand = new DelegateCommand(UpdateSelectedHeaderCmd);
            ResetSelectedHeaderCommand = new DelegateCommand(ResetSelectedHeaderCmd);
            ResetAllHeadersCommand = new DelegateCommand(ResetAllHeadersCmd);

            Headers = new ObservableConcurrentDictionary<string, string>();
            foreach (var item in InitializeHeaders())
            {
                Headers.Add(item.Key, item.Value);
            }
            SelectedHeader = Headers.FirstOrDefault();
        }


        private Dictionary<string, string> InitializeHeaders()
        {
            var exHeaders = _headerNames.PropertiesData;
            if (_settings.HeaderNamesChanged != null)
            {
                foreach (var item in _settings?.HeaderNamesChanged)
                {
                    if (exHeaders.ContainsKey(item.Key))
                        exHeaders[item.Key] = item.Value;
                }
            }
            return exHeaders;
        }


        #region Commands
        public DelegateCommand UpdateSelectedHeaderCommand { get; private set; }
        protected void UpdateSelectedHeaderCmd()
        {
            try
            {
                ValueForSelectedHeader = ValueForSelectedHeader.Trim();
                if (SelectedHeader.Value != ValueForSelectedHeader)
                {
                    if (string.IsNullOrWhiteSpace(ValueForSelectedHeader))
                        throw new Exception("Заголовок не может быть пустым");

                    SelectedHeader = new KeyValuePair<string, string>(SelectedHeader.Key, ValueForSelectedHeader);

                    if (!_settings.HeaderNamesChanged.ContainsKey(SelectedHeader.Key))
                        _settings.HeaderNamesChanged.Add(SelectedHeader.Key, ValueForSelectedHeader);
                    else
                        _settings.HeaderNamesChanged[SelectedHeader.Key] = ValueForSelectedHeader;

                    Headers[SelectedHeader.Key] = _headerNames.PropertiesData[SelectedHeader.Key] = ValueForSelectedHeader;
                    

                    _notificationManager.NotifyInformationAsync("Обновлено");
                }
            }
            catch (Exception ex)
            {
                _notificationManager.NotifyErrorAsync(ex.Message);
            }
        }

        public DelegateCommand ResetSelectedHeaderCommand { get; private set; }
        protected void ResetSelectedHeaderCmd()
        {
            try
            {
                if (_settings.HeaderNamesChanged == null || !_settings.HeaderNamesChanged.Any())
                {
                    _notificationManager.NotifyWarningAsync("Изменения отсутствуют");
                    return;
                }

                if (SelectedHeader.Key != null)
                {
                    if (_settings.HeaderNamesChanged.Remove(SelectedHeader.Key))
                    {
                        Headers[SelectedHeader.Key] 
                            = _headerNames.PropertiesData[SelectedHeader.Key] 
                            = ValueForSelectedHeader
                            = _headerNames.GetFieldValueByPropName(_headerNames, SelectedHeader.Key);

                        SelectedHeader = new KeyValuePair<string, string>(SelectedHeader.Key, Headers[SelectedHeader.Key]);
                        _notificationManager.NotifyInformationAsync("Обновлено");
                    }
                    else throw new Exception("Данный параметр не менялся");
                }
            }
            catch (Exception ex)
            {
                _notificationManager.NotifyErrorAsync(ex.Message);
            }
        }

        public DelegateCommand ResetAllHeadersCommand { get; private set; }
        protected void ResetAllHeadersCmd()
        {
            try
            {
                if (_settings.HeaderNamesChanged != null && _settings.HeaderNamesChanged.Any())
                {
                    while (_settings.HeaderNamesChanged.Count > 0)
                    {
                        var header = _settings.HeaderNamesChanged.First();

                        Headers[header.Key]
                            = _headerNames.PropertiesData[header.Key]
                            = _headerNames.GetFieldValueByPropName(_headerNames, header.Key);

                        _settings.HeaderNamesChanged.Remove(header.Key);
                    }                    

                    SelectedHeader = new KeyValuePair<string, string>(SelectedHeader.Key, Headers[SelectedHeader.Key]);
                    _notificationManager.NotifyInformationAsync("Обновлено");
                }
                else _notificationManager.NotifyWarningAsync("Изменения отсутствуют");
            }
            catch (Exception ex)
            {
                _notificationManager.NotifyErrorAsync(ex.Message);
            }
        }
        #endregion
    }
}
