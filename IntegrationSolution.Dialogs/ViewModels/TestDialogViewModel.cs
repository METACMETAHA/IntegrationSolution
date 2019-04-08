using DialogConstruction.Commands;
using DialogConstruction.Implementations;
using System;
using System.Windows.Input;

namespace IntegrationSolution.Dialogs.ViewModels
{
    public class TestDialogViewModel : DialogViewModel<string>
    {
        private string _text;

        public TestDialogViewModel()
        {
            OkCommand = new RelayCommand(OnOk, CanOk);
            CancelCommand = new RelayCommand(OnCancel);
            _text = String.Empty;

            AddValidationRule(() => Text, text => !String.IsNullOrEmpty(text), "Text must not be empty");
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    ValidateAllRules();
                    NotifyOfPropertyChange(() => Text);
                }
            }
        }

        public ICommand OkCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        private bool CanOk()
        {
            return !HasErrors;
        }

        private void OnOk()
        {
            Close(Text);
        }

        private void OnCancel()
        {
            Close(null);
        }
    }
}
