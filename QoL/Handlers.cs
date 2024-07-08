using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using Terraria.ID;

namespace QoL
{
    public static class Handlers
    {
        public static void InitializeHandlers()
        {
            ServerApi.Hooks.GamePostInitialize.Register(QoL.Instance, OnGamePostInitialize);
            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.ServerLeave.Register(QoL.Instance, OnServerLeave);


            if (QoL.Config.LockDungeonChestsTillSkeletron || QoL.Config.LockShadowChestsTillSkeletron)
            {
                GetDataHandlers.ChestOpen += OnChestOpen;
            }

            if (QoL.Config.QueenBeeRangeCheck)
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

            if (QoL.Config.QueenBeeRangeCheck)
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
            if (Commands.OngoingVoters.ContainsKey(TShock.Players[args.Who].Name))
            {
                Commands.OngoingVoteCount += Commands.OngoingVoters[TShock.Players[args.Who].Name] ? -1 : 1;
                Commands.OngoingVoters.Remove(TShock.Players[args.Who].Name);
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
            else if (QoL.Config.LockShadowChestsTillSkeletron &&
                Main.tile[args.X, args.Y].type == 21 &&
                Main.tile[args.X, args.Y].frameX / 36 == 3 &&
                !NPC.downedBoss3)
            {
                args.Handled = true;
            }
        }

        private static void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            if (Main.npc[args.NpcId].netID == NPCID.QueenBee)
            {
                QoL.QueenBeeIndexList.Add(args.NpcId);
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            int[] indexesToRemove = { };
            foreach (int queenIndex in QoL.QueenBeeIndexList)
            {
                NPC queenBee = Main.npc[queenIndex];

                bool isFarAway = true;
                for (int i = 0; i < TShock.Config.Settings.MaxSlots + TShock.Config.Settings.ReservedSlots; i++)
                {
                    TSPlayer? plr = TShock.Players[i];
                    if (plr != null && plr.Active && !plr.Dead && queenBee.position.WithinRange(plr.TPlayer.position, 16 * 450))
                    {
                        isFarAway = false;
                    }
                }

                if (isFarAway)
                {
                    queenBee.active = false;
                    queenBee.type = 0;
                    indexesToRemove.Append(queenIndex);
                    NetMessage.SendData((int)PacketTypes.NpcUpdate, number: queenIndex);
                }
            }

            foreach (int index in indexesToRemove)
            {
                QoL.QueenBeeIndexList.Remove(index);
            }
        }
    }
}