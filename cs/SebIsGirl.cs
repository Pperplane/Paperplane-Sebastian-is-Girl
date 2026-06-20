using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Newtonsoft.Json.Linq; 
using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using System.Text;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace SebastianToSabrina
{
    public class ModEntry : Mod
    {
        public static IMonitor ModMonitor;
        public override void Entry(IModHelper helper)
        {
            try
            {
                ModMonitor = this.Monitor;

                var harmony = new Harmony("Paperplane01.SebastianIsGirl");
                harmony.PatchAll();

                this.Monitor.Log("Successfully applied Island Schedule patches!", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to apply Harmony patches: {ex}", LogLevel.Error);
            }

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
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
                    
                    string configPath = Path.Combine(targetFolder, "config.json");

                    if (File.Exists(configPath))
                    {
                        string jsonText = File.ReadAllText(configPath);
                        JObject configJson = JObject.Parse(jsonText);

                        var settingToken = configJson[targetSettingName];
                        if (settingToken != null && settingToken.Type != JTokenType.Null && settingToken.Value<bool>() == true)
                        {
                            configJson[targetSettingName] = false;
                            File.WriteAllText(configPath, configJson.ToString());
                            this.Monitor.Log($"[Sebastian to Sabrina] Automatically disabled '{targetSettingName}' to avoid visual conflicts.", LogLevel.Info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Monitor.Log($"[Sebastian to Sabrina] Failed to auto-adjust config: {ex.Message}", LogLevel.Warn);
                }
            }
        }
    }
}
    
