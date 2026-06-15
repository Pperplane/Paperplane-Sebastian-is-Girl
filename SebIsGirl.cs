using System;
using System.IO;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Newtonsoft.Json.Linq; 

namespace SebastianToSabrina
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            // Only hook into the launched event to handle the file edit
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            string targetModID = "Poltergeister.SeasonalCuteCharacters"; 
            string targetSettingName = "SlightlyCuterSebastian"; 

            var otherMod = this.Helper.ModRegistry.Get(targetModID);
            if (otherMod != null)
            {
                try
                {
                    string modsDirectory = Path.Combine(this.Helper.DirectoryPath, "..");
                    string targetFolder = Path.Combine(modsDirectory, "[CP] Seasonal Cute Characters");
                    
                    if (!Directory.Exists(targetFolder))
                    {
                        
                        targetFolder = Path.Combine(modsDirectory, targetModID);
                    }
                    
                    this.Monitor.Log($"[Sebastian to Sabrina] {targetFolder} exists");
                    string configPath = Path.Combine(targetFolder, "config.json");

                    if (File.Exists(configPath))
                    {
                        string jsonText = File.ReadAllText(configPath);
                        JObject configJson = JObject.Parse(jsonText);

                        var settingToken = configJson[targetSettingName];
                        this.Monitor.Log($"{settingToken}");
                        if (settingToken != null && settingToken.Type != JTokenType.Null && settingToken.Value<bool>() == true)
                        {
                            configJson[targetSettingName] = false;

                            File.WriteAllText(configPath, configJson.ToString());
                            
                            this.Monitor.Log($"[Sebastian to Sabrina] Automatically disabled '{targetSettingName}' in '{targetModID}' config to prevent identity conflicts.", LogLevel.Info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"[Sebastian to Sabrina] Failed to auto-adjust the config for {targetModID}. Error: {ex.Message}", LogLevel.Warn);
                }
            }
        }
    }
}