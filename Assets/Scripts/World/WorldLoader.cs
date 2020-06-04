using System.IO;
using Newtonsoft.Json;
using Settings;
using UnityEngine;
using World.Model;
using World.Terrain;

namespace World
{
    public class WorldLoader : MonoBehaviour
    {
        private M2Handler m2Handler;
        private ADTHandler adtHandler;

        public GameObject ChunkPrefab;

        public void Start()
        {
            // Initialize settings.
            InitializeSettings();

            var m2Parent = GameObject.Find("M2");
            m2Handler = new M2Handler(m2Parent);
            
            var adtParent = GameObject.Find("ADT");
            adtHandler = new ADTHandler(adtParent, ChunkPrefab);
        }

        public void Update()
        {
            m2Handler?.Update();
            adtHandler?.Update();
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