﻿using IntegrationSolution.Common.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Common.Models
{
    public class DatesFromToContext : PropertyChangedBase
    {
        private DateTime _fromDate;
        public DateTime FromDate
        {
            get { return _fromDate; }
            set
            {
                _fromDate = value;
                NotifyOfPropertyChange();
            }
        }


        private DateTime _toDate;
        public DateTime ToDate
        {
            get { return _toDate; }
            set
            {
                _toDate = value;
                NotifyOfPropertyChange();
            }
        }


        public DatesFromToContext()
        {
            FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ToDate = DateTime.Now;
        }
    }
}
