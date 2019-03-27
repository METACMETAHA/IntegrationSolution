using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Common
{
    public class StyleExcel
    {
        #region Variables
        public Font HeadersFont { get; private set; }
        public Color HeadersBackgroundColor { get; private set; }
        #endregion


        public StyleExcel()
        { Initialize(); }


        public void Initialize()
        {
            HeadersFont = new Font("Arial", 11, FontStyle.Bold);
            HeadersBackgroundColor = Color.FromArgb(255, 192, 0);
        }
    }
}
