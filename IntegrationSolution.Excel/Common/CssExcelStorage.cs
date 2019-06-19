using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationSolution.Excel.Common
{
    public class CssExcelStorage
    {
        #region Variables
        public Font HeadersFont { get; private set; }
        public Color HeadersBackgroundColor { get; private set; }

        public Color GreenColor { get; private set; }
        public Color RedColor { get; private set; }
        public Color LightRedColor { get; private set; }
        public Color LightGreenColor { get; private set; }
        public Color LightYellowColor { get; private set; }
        #endregion


        public CssExcelStorage()
        { Initialize(); }


        public void Initialize()
        {
            HeadersFont = new Font("Arial", 11, FontStyle.Bold);
            HeadersBackgroundColor = Color.FromArgb(255, 192, 0);
            GreenColor = Color.FromArgb(153, 255, 173);
            RedColor = Color.FromArgb(255, 89, 89);
            LightRedColor = Color.FromArgb(255, 196, 196);
            LightGreenColor = Color.FromArgb(219, 255, 210);
            LightYellowColor = Color.FromArgb(250, 235, 168);
        }
    }
}
