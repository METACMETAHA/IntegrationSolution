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
    public class LoadingFilesViewModel : VMLocalBase
    {
        public LoadingFilesViewModel(IUnityContainer container, IEventAggregator ea) : base(container, ea)
        {
            this.Title = "Loading";
        }

        public override bool MoveNext()
        {
            if (!this.CanGoNext)
                return false;


            return true;
        }


        public override bool MoveBack() => this.CanGoBack;


        protected override void Submit()
        {
            //if (SiteData.IdSite == 0 || String.IsNullOrWhiteSpace(SiteData.SiteName))
            //{
            //    Error = new Common.DataTypes.Error()
            //    {
            //        IsError = true,
            //        ErrorDescription = "Check input values"
            //    };
            //}
            //else
            //{
            //    this.siteRepository = new Persistence.SiteRepository(this.SiteData.IpAddress);

            //    //AddSiteToCenter
            //    this.Error =
            //        centerRepository.AddSiteToCenter(SiteID: SiteData.IdSite, SiteName: SiteData.SiteName, SiteIPAddress: SiteData.IpAddress);
            //}


            //if (!this.Error.IsError)
            //{
            //    //UpdateSiteIDOnSite
            //    Error =
            //        siteRepository.UpdateSiteIDOnSite(SiteID: SiteData.IdSite, SiteName: SiteData.SiteName, SiteIPAddress: SiteData.IpAddress);

            //    if (!this.Error.IsError)
            //    {
            //        Error.ErrorDescription = "Great";
            //        IsFinished = true;
            //        this.CanGoNext = true;
            //    }
            //}

            base.NotifyOnUpdateEvents();
        }
    }
}
