using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Settings;
using UnityEngine;

namespace World
{
    public class WorldLoader : MonoBehaviour
    {
        public Dictionary<uint, GameObject> LoadedM2s = new Dictionary<uint, GameObject>();
        public List<int> LoadedUniqueIds = new List<int>();

        public void Start()
        {
            // Initialize settings.
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            if (!File.Exists("settings.json"))
            {
                var config = new ModelViewerConfig();
                File.WriteAllText("settings.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            
            SettingsManager<ModelViewerConfig>.Initialize("settings.json");
        }
    }
}