using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace fulcrum
{
    public class FulcrumTools : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public static string apiBase = "https://api.fulcrumapp.com/api/v2";
        public static FulcrumWindow window;
        public FulcrumTools()
        {
        }
        protected override void OnClick()
        {
            try
            {
                window = new fulcrum.FulcrumWindow();
                window.ShowDialog();
            }
            catch(Exception e)
            {
                string ouch = e.Message;
            }
            
        }

        protected override void OnUpdate()
        {
            this.Enabled = true;
        }
    }
}
