using DialogConstruction.Commands;
using DialogConstruction.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IntegrationSolution.Dialogs.ViewModels
{
    public class InstructionsForTokenVM : DialogViewModel
    {
        public InstructionsForTokenVM()
        {
            OkCommand = new RelayCommand(OnOk, CanOk);
            HyperlinkCommand = new RelayCommand<string>(LinkCmd);
        }

        private bool CheckInput()
        {
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
            Close();
        }


        public ICommand HyperlinkCommand { get; set; }
        private void LinkCmd(string link)
        {
            System.Diagnostics.Process.Start(new Uri(link).ToString());
        }
        #endregion
    }
}
