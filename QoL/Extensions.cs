using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using TShockAPI;

namespace QoL;

public static class Extensions
{
    public static void ScaleStats(this NPC npc, GameModeData? gameModeData = null, float? strengthOverride = null)
    {
        gameModeData ??= Main.GameModeInfo;

        if ((!NPCID.Sets.NeedsExpertScaling.IndexInRange(npc.type) || !NPCID.Sets.NeedsExpertScaling[npc.type]) && (npc.lifeMax <= 5 || npc.damage == 0 || npc.friendly || npc.townNPC))
        {
            return;
        }
        float num = 1f;
        if (strengthOverride.HasValue)
        {
            num = strengthOverride.Value;
        }
        else if (gameModeData.IsJourneyMode)
        {
            CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
            if (power != null && power.GetIsUnlocked())
            {
                num = power.StrengthMultiplierToGiveNPCs;
            }
        }
        float num2 = num;
        if (gameModeData.IsJourneyMode && Main.getGoodWorld)
        {
            num += 1f;
        }
        NPCStrengthHelper nPCStrengthHelper = new NPCStrengthHelper(gameModeData, num, Main.getGoodWorld);
        if (nPCStrengthHelper.IsExpertMode)
        {
            npc.ScaleStats_ApplyExpertTweaks();
        }
        npc.ScaleStats_ApplyGameMode(gameModeData);

        /*
                if (Main.getGoodWorld && nPCStrengthHelper.ExtraDamageForGetGoodWorld)
                {
                    npc.damage += npc.damage / 3;
                }
        */
        if (nPCStrengthHelper.IsExpertMode)
        {
            int num3 = 1;
            num3 = npc.statsAreScaledForThisManyPlayers;
            NPC.GetStatScalingFactors(num3, out var balance, out var boost);
            float bossAdjustment = 1f;
            if (nPCStrengthHelper.IsMasterMode)
            {
                bossAdjustment = 0.85f;
            }
            npc.ScaleStats_ApplyMultiplayerStats(num3, balance, boost, bossAdjustment);
        }
        npc.ScaleStats_UseStrengthMultiplier(num);
        npc.strengthMultiplier = num2;
        if ((npc.type < 0 || !NPCID.Sets.ProjectileNPC[npc.type]) && npc.lifeMax < 6)
        {
            npc.lifeMax = 6;
        }
        npc.life = npc.lifeMax;
        npc.defDamage = npc.damage;
        npc.defDefense = npc.defense;
    }
}