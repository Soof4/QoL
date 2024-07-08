using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QoL
{
    public class Config
    {
        public bool QueenBeeRangeCheck = true;
        public bool LockDungeonChestsTillSkeletron = true;
        public bool LockShadowChestsTillSkeletron = true;
        public int VotebanTimeInMinutes = 60;
        public bool DisableQuickStack = false;
        public bool EnableNameWhitelist = false;
        public string[] WhitelistedNames = new string[0];

        public void Write()
        {
            File.WriteAllText(QoL.ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Reload()
        {
            Config? c = null;

            if (File.Exists(QoL.ConfigPath)) c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(QoL.ConfigPath));

            if (c == null)
            {
                c = new Config();
                File.WriteAllText(QoL.ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));
            }

            return c;
        }
    }
}