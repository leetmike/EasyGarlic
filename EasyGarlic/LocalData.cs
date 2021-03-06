﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyGarlic {
    // Only use this on load or saving
    public class LocalData {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // App version
        // TODO: Make AutoUpdate system
        public string version = Config.VERSION;
        // Where to fetch new miner data
        public string dataURL = Config.DATA_URL;
        // Currently installed miners
        public Dictionary<string, Miner> installed = new Dictionary<string, Miner>();

        public string savedAddress;

        // TODO: Make SavedPool (currently it's just a value that's not used)
        // Could do that by saving a PoolData object on MainWindow and use those values whenever Custom is selected (rather than editing the one from the List)
        private PoolData savedPool;

        public bool Exists()
        {
            return File.Exists(Path.GetLocalDataFile());
        }

        // TODO: Make a system so that if the version do not match, then it needs to delete the data.json file (can probably do that using the UpdateManager later)

        public void SetupDefault()
        {
            version = Config.VERSION;
            dataURL = Config.DATA_URL;
        }

        // Save the file content into JSON with async methods
        public async Task SaveAsync()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] encodedJson = Encoding.Unicode.GetBytes(json);
            string savePath = Path.GetLocalDataFile();

            await Task.Run(() => { File.WriteAllText(savePath, json, Encoding.UTF8); });
        }

        // Load the file content from JSON with async methods
        public async Task LoadAsync()
        {
            string savePath = Path.GetLocalDataFile();

            string json = await Task.Run(() => { return File.ReadAllText(savePath, Encoding.UTF8); });

            logger.Debug("LocalData JSON: " + json);

            LoadData(json);
        }

        // Loads the given JSON data into a LocalData object
        private void LoadData(string jsonData)
        {
            JObject obj = JObject.Parse(jsonData);
            LocalData deserialized = obj.ToObject<LocalData>();
            installed = deserialized.installed;
            savedAddress = deserialized.savedAddress;
        }

    }
}
