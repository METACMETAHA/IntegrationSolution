﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;

namespace DsxGridCtrl
{
    public class xConverter : IValueConverter 
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        { 
            return value;
        } 
     
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        { 
            if (value != null && value.ToString().Length>0) 
            { 
                return ((DateTime)value).ToString("d", CultureInfo.CurrentCulture); 
            } 
            else 
            { 
                return String.Empty; 
            } 
        } 
    } 
}
