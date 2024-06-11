using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QoL {
    public class Config {
        public bool QueenBeeRangeCheck = true;
        public bool LockDungeonChestsTillSkeletron = true;
        public bool LockShadowChestsTillSkeletron = true;
        public bool EnableNewItemCommand = true;
        public bool EnableLuckCommand = true;
        public bool EnableVotebanCommand = true;
        public int VotebanTimeInMinutes = 60;
        public bool EnableVotekickCommand = true;
        public bool DisableQuickStack = false;
        public bool EnableNameWhitelist = false;
        public string[] WhitelistedNames = new string[0];
        
        public void Write() {
            File.WriteAllText(QoL.ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read() {
            if (!File.Exists(QoL.ConfigPath)) {
                return new Config();
            }
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(QoL.ConfigPath));
        }
    }
}