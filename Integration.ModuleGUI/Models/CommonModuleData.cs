using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.ModuleGUI.Models
{
    public class CommonModuleData : BindableBase
    {
        #region Properties
        /// <summary>
        /// IpAddress From Field
        /// </summary>
        private string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set { SetProperty(ref _ipAddress, value); }
        }


        private int _idSite;
        public int IdSite
        {
            get { return _idSite; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Site ID must be > 0");
                SetProperty(ref _idSite, value);
            }
        }


        private string _siteName;
        public string SiteName
        {
            get { return _siteName; }
            set
            { SetProperty(ref _siteName, value); }
        }


        private int _copyFromSiteId;
        public int CopyFromSiteId
        {
            get { return _copyFromSiteId; }
            set
            {
                if (value < 0 || value == IdSite)
                    throw new ArgumentException("Site ID must be > 0 and not equals to current Site ID");
                SetProperty(ref _copyFromSiteId, value);
            }
        }


        private int _zNumber;
        public int ZNumber
        {
            get { return _zNumber; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Z number must be > 0");
                SetProperty(ref _zNumber, value);
            }
        }
        #endregion Properties


        public CommonModuleData()
        { }
    }
}
