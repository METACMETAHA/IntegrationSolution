using DialogConstruction.Commands;
using DialogConstruction.Implementations;
using IntegrationSolution.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IntegrationSolution.Dialogs.ViewModels
{
    public class FuelPriceInputDialogVM : DialogViewModel<FuelPrice>
    {
        public FuelPrice FuelPrice { get; set; }

        public FuelPriceInputDialogVM()
        {
            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
            AddValidationRule(() => FuelPrice, text => FuelPrice != null && FuelPrice.GasCost > 5, "Text must not be empty");

            FuelPrice = new FuelPrice()
            {
                GasCost = 10
            };
        }


        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        private bool CanOk()
        {
            return !HasErrors;
        }

        private void OnOk()
        {
            Close(FuelPrice);
        }

        private void OnCancel()
        {
            Close(null);
        }
    }
}
