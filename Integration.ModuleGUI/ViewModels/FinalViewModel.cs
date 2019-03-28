using Integration.ModuleGUI.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Integration.ModuleGUI.ViewModels
{
    public class FinalViewModel : VMLocalBase
    {
        public FinalViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            CanGoBack = true;
            CanGoNext = false;
            this.Title = "Done";
        }

        public override bool MoveBack()
        {
            return true;
        }

        public override bool MoveNext()
        {
            return true;
        }

        protected override void Submit()
        {

        }
    }
}
