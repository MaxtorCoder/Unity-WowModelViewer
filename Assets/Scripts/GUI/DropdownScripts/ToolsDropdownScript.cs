using Interfaces;
using UnityEngine;
using Util;

namespace GUI.DropdownScripts
{
    public class ToolsDropdownScript : DropdownBase
    {
        public ToolsDropdownScript(GameObject ui) : base(ui)
        {
            Events.Add("OpenMapImport", OpenMapImport);
        }

        private void OpenMapImport()
        {
            var mapImport = UI.FindObject("MapImport");
            mapImport.SetActive(!mapImport.activeSelf);
        }
    }
}