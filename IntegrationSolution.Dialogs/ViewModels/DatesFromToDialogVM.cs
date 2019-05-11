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

        public DatesFromToDialogVM(IUnityContainer unity) : base(unity)
        {
            Dates = new DatesFromToContext();
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
