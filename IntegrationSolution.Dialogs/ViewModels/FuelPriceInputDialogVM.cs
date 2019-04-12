using DialogConstruction.Commands;
using DialogConstruction.Implementations;
using IntegrationSolution.Common.Models;
using System;
using System.Windows.Input;
using Unity;

namespace IntegrationSolution.Dialogs.ViewModels
{
    public class FuelPriceInputDialogVM : DialogViewModel<FuelPrice>
    {
        private FuelPrice _fuelPrice;
        public FuelPrice FuelPrice
        {
            get { return _fuelPrice; }
            set
            {
                _fuelPrice = value;
                NotifyOfPropertyChange();
            }
        }

        public FuelPriceInputDialogVM(IUnityContainer unity) : base(unity)
        {
            FuelPrice = unity.Resolve<FuelPrice>();
            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
            //AddValidationRule(() => FuelPrice, x => x.HasErrors , "Text must not be empty");
        }

        private bool CheckInput()
        {
            if (FuelPrice == null)
                return false;

            if (FuelPrice.DiselCost < 1
                || FuelPrice.GasCost < 1
                || FuelPrice.LPGCost < 1)
                return false;

            if (FuelPrice.DiselCost > 1000
                || FuelPrice.GasCost > 1000
                || FuelPrice.LPGCost > 1000)
                return false;
            
            return true;
        }

        #region Commands
        public ICommand OkCommand { get; set; }
        private bool CanOk()
        {
            return CheckInput();
            //return !HasErrors;
        }
        private void OnOk()
        {
            Close(FuelPrice);
        }


        public ICommand CancelCommand { get; set; }
        private void OnCancel()
        {
            Close(null);
        }
        #endregion
    }
}
