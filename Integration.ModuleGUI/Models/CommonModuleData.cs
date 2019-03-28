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
        private string _pathToMainFile;
        public string PathToMainFile
        {
            get { return _pathToMainFile; }
            set { SetProperty(ref _pathToMainFile, value); }
        }


        private string _pathToPathListFile;
        public string PathToPathListFile
        {
            get { return _pathToPathListFile; }
            set
            { SetProperty(ref _pathToPathListFile, value); }
        }
        #endregion Properties


        public CommonModuleData()
        { }
    }
}
