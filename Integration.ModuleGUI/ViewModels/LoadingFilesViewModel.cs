using Integration.ModuleGUI.Models;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class LoadingFilesViewModel : VMLocalBase
    {
        public LoadingFilesViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            this.Title = "Загрузка файлов";
            LoadFileCommand = new DelegateCommand(Load);
        }

        public override bool MoveNext()
        {
            if (!this.CanGoNext)
                return false;


            return true;
        }


        public override bool MoveBack() => this.CanGoBack;


        


        #region Commands
        public DelegateCommand LoadFileCommand { get; private set; }
        protected void Load()
        {
            

            base.NotifyOnUpdateEvents();
        }
        #endregion
    }
}
