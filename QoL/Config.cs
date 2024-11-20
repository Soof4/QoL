using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QoL;

public class Config
{
    public bool FragmentsFunctionLikeTreasureBags = true;
    public bool QueenBeeRangeCheck = true;
    public bool DeerclopsRangeCheck = true;
    public bool LockDungeonChestsTillSkeletron = true;
    public bool LockShadowChestsTillSkeletron = true;
    public bool LockTempleChestsTillPlantera = true;
    public int VotebanTimeInMinutes = 60;
    public int VoteDurationInMinutes = 1;
    public bool DisableQuickStack = false;
    public bool EnableNameWhitelist = false;
    public string[] WhitelistedNames = new string[0];
    public Dictionary<string, int> CommandCooldowns = new Dictionary<string, int>();

    public void Write()
    {
        File.WriteAllText(QoL.ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static Config Reload()
    {
        Config? c = null;

        if (File.Exists(QoL.ConfigPath)) c = JsonConvert.DeserializeObject<Config>(File.ReadAllText(QoL.ConfigPath));

        c ??= new Config();

        File.WriteAllText(QoL.ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));

        return c;
    }
}