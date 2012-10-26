using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace fulcrum
{
    public class FulcrumSettings : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public FulcrumSettings()
        {
        }

        protected override void OnClick()
        {
            fulcrumSettingsWindow settingsWindow = new fulcrumSettingsWindow();
            settingsWindow.ShowDialog();
        }
        protected override void OnUpdate()
        {
            Enabled = ArcMap.Application != null;
        }
    }

}
