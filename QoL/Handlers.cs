using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace QoL;

public static class Handlers
{
    public static void InitializeHandlers()
    {
        ServerApi.Hooks.GamePostInitialize.Register(QoL.Instance, OnGamePostInitialize);
        ServerApi.Hooks.ServerLeave.Register(QoL.Instance, OnServerLeave);
        GeneralHooks.ReloadEvent += OnReload;

        if (QoL.Config.FragmentsFunctionLikeTreasureBags)
        {
            ServerApi.Hooks.NpcLootDrop.Register(QoL.Instance, OnNpcLootDrop);
        }

        if (QoL.Config.LockDungeonChestsTillSkeletron || QoL.Config.LockShadowChestsTillSkeletron)
        {
            GetDataHandlers.ChestOpen += OnChestOpen;
        }

        if (QoL.Config.QueenBeeRangeCheck || QoL.Config.DeerclopsRangeCheck)
        {
            ServerApi.Hooks.GameUpdate.Register(QoL.Instance, OnGameUpdate);
            ServerApi.Hooks.NpcSpawn.Register(QoL.Instance, OnNpcSpawn);
        }

        if (QoL.Config.EnableNameWhitelist)
        {
            ServerApi.Hooks.ServerJoin.Register(QoL.Instance, OnServerJoin);
        }

        if (QoL.Config.DisableQuickStack)
        {
            ServerApi.Hooks.ItemForceIntoChest.Register(QoL.Instance, OnItemForceIntoChest);
        }
    }

    private static void OnReload(ReloadEventArgs args)
    {
        QoL.Config = Config.Reload();
        ReloadHandlers();
        args.Player.SendSuccessMessage("[QoL] Reloaded.");
    }

    public static void DisposeHandlers()
    {
        ServerApi.Hooks.GamePostInitialize.Deregister(QoL.Instance, OnGamePostInitialize);
        GeneralHooks.ReloadEvent -= OnReload;
        ServerApi.Hooks.ServerLeave.Deregister(QoL.Instance, OnServerLeave);


        if (QoL.Config.LockDungeonChestsTillSkeletron || QoL.Config.LockShadowChestsTillSkeletron)
        {
            GetDataHandlers.ChestOpen -= OnChestOpen;
        }

        if (QoL.Config.FragmentsFunctionLikeTreasureBags)
        {
            ServerApi.Hooks.NpcLootDrop.Deregister(QoL.Instance, OnNpcLootDrop);
        }

        if (QoL.Config.QueenBeeRangeCheck)
        {
            ServerApi.Hooks.GameUpdate.Deregister(QoL.Instance, OnGameUpdate);
            ServerApi.Hooks.NpcSpawn.Deregister(QoL.Instance, OnNpcSpawn);
        }

        if (QoL.Config.EnableNameWhitelist)
        {
            ServerApi.Hooks.ServerJoin.Deregister(QoL.Instance, OnServerJoin);
        }

        if (QoL.Config.DisableQuickStack)
        {
            ServerApi.Hooks.ItemForceIntoChest.Deregister(QoL.Instance, OnItemForceIntoChest);
        }
    }

    public static void ReloadHandlers()
    {
        if (QoL.Config.LockDungeonChestsTillSkeletron || QoL.Config.LockShadowChestsTillSkeletron)
        {
            GetDataHandlers.ChestOpen += OnChestOpen;
        }
        else
        {
            GetDataHandlers.ChestOpen -= OnChestOpen;
        }

        if (QoL.Config.FragmentsFunctionLikeTreasureBags)
        {
            ServerApi.Hooks.NpcLootDrop.Register(QoL.Instance, OnNpcLootDrop);
        }
        else
        {
            ServerApi.Hooks.NpcLootDrop.Deregister(QoL.Instance, OnNpcLootDrop);
        }

        if (QoL.Config.QueenBeeRangeCheck || QoL.Config.DeerclopsRangeCheck)
        {
            ServerApi.Hooks.GameUpdate.Register(QoL.Instance, OnGameUpdate);
            ServerApi.Hooks.NpcSpawn.Register(QoL.Instance, OnNpcSpawn);
        }
        else
        {
            ServerApi.Hooks.GameUpdate.Deregister(QoL.Instance, OnGameUpdate);
            ServerApi.Hooks.NpcSpawn.Deregister(QoL.Instance, OnNpcSpawn);
        }

        if (QoL.Config.EnableNameWhitelist)
        {
            ServerApi.Hooks.ServerJoin.Register(QoL.Instance, OnServerJoin);
        }
        else
        {
            ServerApi.Hooks.ServerJoin.Deregister(QoL.Instance, OnServerJoin);
        }

        if (QoL.Config.DisableQuickStack)
        {
            ServerApi.Hooks.ItemForceIntoChest.Register(QoL.Instance, OnItemForceIntoChest);
        }
        else
        {
            ServerApi.Hooks.ItemForceIntoChest.Deregister(QoL.Instance, OnItemForceIntoChest);
        }
    }

    private static void OnItemForceIntoChest(ForceItemIntoChestEventArgs args)
    {
        args.Handled = true;
    }

    private static void OnGamePostInitialize(EventArgs args)
    {
        Main.rand ??= new();
    }

    private static void OnServerJoin(JoinEventArgs args)
    {
        if (!QoL.Config.WhitelistedNames.Contains(TShock.Players[args.Who].Name))
        {
            NetMessage.TrySendData((int)PacketTypes.Disconnect, args.Who, -1, Terraria.Localization.NetworkText.FromLiteral(TShock.Config.Settings.WhitelistKickReason));
        }
    }

    public static void OnServerLeave(LeaveEventArgs args)
    {
        VoteService.TryRemoveVote(args.Who);
    }

    private static void OnNpcLootDrop(NpcLootDropEventArgs args)
    {
        //default vanilla config for fragments
        DropOneByOne.Parameters parameters = default(DropOneByOne.Parameters);
        parameters.MinimumItemDropsCount = 12;
        parameters.MaximumItemDropsCount = 20;
        parameters.ChanceNumerator = 1;
        parameters.ChanceDenominator = 1;
        parameters.MinimumStackPerChunkBase = 1;
        parameters.MaximumStackPerChunkBase = 3;
        parameters.BonusMinDropsPerChunkPerPlayer = 0;
        parameters.BonusMaxDropsPerChunkPerPlayer = 0;

        if (Main.expertMode || Main.masterMode)
        {
            parameters.BonusMinDropsPerChunkPerPlayer = 1;
            parameters.BonusMaxDropsPerChunkPerPlayer = 1;
            parameters.MinimumStackPerChunkBase = 1;
            parameters.MaximumStackPerChunkBase = 4;
        }

        NPC npc = Main.npc[args.NpcArrayIndex];
        if (npc.netID == NPCID.LunarTowerSolar || npc.netID == NPCID.LunarTowerVortex || npc.netID == NPCID.LunarTowerNebula || npc.netID == NPCID.LunarTowerStardust)
        {
            for (int plrIndex = 0; plrIndex < 255; plrIndex++)
            {
                Player player = Main.player[plrIndex];
                if (player.active && npc.playerInteraction[plrIndex] && player.RollLuck(1) < 1) //amount of fragments for each player will depend on that player's luck
                {
                    npc.playerInteraction[plrIndex] = false; //set this to false to prevent this hook from running multiple times since it will be called for each chunk of fragments was dropped.
                    int num = Main.rand.Next(parameters.MinimumItemDropsCount, parameters.MaximumItemDropsCount + 1);
                    int minValue = parameters.MinimumStackPerChunkBase + parameters.BonusMinDropsPerChunkPerPlayer;
                    int num2 = parameters.MaximumStackPerChunkBase + parameters.BonusMaxDropsPerChunkPerPlayer;
                    for (int i = 0; i < num; i++)
                    {
                        int x = (int)npc.position.X + Main.rand.Next(npc.width + 1);
                        int y = (int)npc.position.Y + Main.rand.Next(npc.height + 1);
                        int itemIndex = Item.NewItem(npc.GetItemSource_Loot(), x, y, 0, 0, args.ItemId, Main.rand.Next(minValue, num2 + 1), true, -1);
                        Main.timeItemSlotCannotBeReusedFor[itemIndex] = 54000;
                        NetMessage.SendData(90, plrIndex, -1, null, itemIndex);
                        Main.item[itemIndex].active = false;
                    }
                }
            }
            args.Handled = true;
        }
    }

    private static void OnChestOpen(object? sender, GetDataHandlers.ChestOpenEventArgs args)
    {
        if (QoL.Config.LockDungeonChestsTillSkeletron &&
            QoL.DungeonWallIDs.Contains(Main.tile[args.X, args.Y].wall) &&
            !NPC.downedBoss3)
        {
            args.Handled = true;
        }
        if (QoL.Config.LockShadowChestsTillSkeletron &&
            Main.tile[args.X, args.Y].type == 21 &&
            Main.tile[args.X, args.Y].frameX / 36 == 3 &&
            !NPC.downedBoss3)
        {
            args.Handled = true;
        }
        if (QoL.Config.LockTempleChestsTillPlantera &&
            Main.tile[args.X, args.Y].type == 21 &&
            Main.tile[args.X, args.Y].frameX / 36 == 16 &&
            !NPC.downedPlantBoss)
        {
            args.Handled = true;
        }
    }

    private static void OnNpcSpawn(NpcSpawnEventArgs args)
    {
        if (Main.npc[args.NpcId].netID == NPCID.QueenBee && QoL.Config.QueenBeeRangeCheck)
        {
            QoL.QueenBeeIndexList.Add(args.NpcId);
        }
        else if (Main.npc[args.NpcId].netID == NPCID.Deerclops && QoL.Config.DeerclopsRangeCheck)
        {
            QoL.DeerclopsIndexList.Add(args.NpcId);
        }
    }

    private static void OnGameUpdate(EventArgs args)
    {
        // Queen bee despawn
        if (QoL.Config.QueenBeeRangeCheck)
        {
            int[] qbIndexesToRemove = { };
            foreach (int queenIndex in QoL.QueenBeeIndexList)
            {
                NPC queenBee = Main.npc[queenIndex];

                bool isFarAway = true;
                foreach (TSPlayer plr in TShock.Players)
                {
                    if (plr != null && plr.Active && !plr.Dead && queenBee.position.WithinRange(plr.TPlayer.position, 16 * 450))
                    {
                        isFarAway = false;
                    }
                }

                if (isFarAway)
                {
                    queenBee.active = false;
                    queenBee.type = 0;
                    qbIndexesToRemove.Append(queenIndex);
                    NetMessage.SendData((int)PacketTypes.NpcUpdate, number: queenIndex);
                }
            }

            foreach (int index in qbIndexesToRemove)
            {
                QoL.QueenBeeIndexList.Remove(index);
            }
        }

        // Deerclops despawn
        if (QoL.Config.DeerclopsRangeCheck)
        {
            int[] dcIndexesToRemove = { };
            foreach (int dcIndex in QoL.DeerclopsIndexList)
            {
                NPC dc = Main.npc[dcIndex];

                bool isFarAway = true;
                foreach (TSPlayer plr in TShock.Players)
                {
                    if (plr != null && plr.Active && !plr.Dead && dc.position.WithinRange(plr.TPlayer.position, 16 * 450))
                    {
                        isFarAway = false;
                    }
                }

                if (isFarAway)
                {
                    dc.active = false;
                    dc.type = 0;
                    dcIndexesToRemove.Append(dcIndex);
                    NetMessage.SendData((int)PacketTypes.NpcUpdate, number: dcIndex);
                }
            }

            foreach (int index in dcIndexesToRemove)
            {
                QoL.QueenBeeIndexList.Remove(index);
            }
        }
    }
}