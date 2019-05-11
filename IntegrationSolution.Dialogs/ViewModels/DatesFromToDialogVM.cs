using DialogConstruction.Commands;
using DialogConstruction.Implementations;
using IntegrationSolution.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;

namespace IntegrationSolution.Dialogs.ViewModels
{
    public class DatesFromToDialogVM : DialogViewModel<DatesFromToContext>
    {
        private DatesFromToContext _dates;
        public DatesFromToContext Dates
        {
            get { return _dates; }
            set
            {
                _dates = value;
                NotifyOfPropertyChange();
            }
        }

        #region RadioButtons
        private bool _perMonth;
        public bool PerMonth
        {
            get { return _perMonth; }
            set
            {
                if (value)
                {
                    Dates.FromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    Dates.ToDate = DateTime.Now;
                }
                _perMonth = value;
                NotifyOfPropertyChange();
            }
        }


        private bool _perWeek;
        public bool PerWeek
        {
            get { return _perWeek; }
            set
            {
                if (value)
                {
                    DayOfWeek day = DateTime.Now.DayOfWeek;
                    int days = day - DayOfWeek.Monday;
                    Dates.FromDate = DateTime.Now.AddDays(-days);
                    Dates.ToDate = DateTime.Now;
                }

                _perWeek = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _perDay;
        public bool PerDay
        {
            get { return _perDay; }
            set
            {
                if (value)
                {
                    Dates.FromDate = DateTime.Today;
                    Dates.ToDate = DateTime.Now;
                }

                _perDay = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _custom;
        public bool Custom
        {
            get { return _custom; }
            set
            {
                _custom = value;
                NotifyOfPropertyChange();
            }
        }
        #endregion


        public DatesFromToDialogVM(IUnityContainer unity) : base(unity)
        {
            Dates = new DatesFromToContext();
            PerMonth = true;
            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
        }


        private bool CheckInput()
        {
            if (Dates == null)
                return false;
            
            if (Dates.FromDate >= Dates.ToDate)
                return false;

            return true;
        }


        #region Commands
        public ICommand OkCommand { get; set; }
        private bool CanOk()
        {
            return CheckInput();
        }
        private void OnOk()
        {
            Close(Dates);
        }


        public ICommand CancelCommand { get; set; }
        private void OnCancel()
        {
            Close(null);
        }
        #endregion
    }
}
