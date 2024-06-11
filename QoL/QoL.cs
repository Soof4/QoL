using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Localization;


namespace QoL
{

    [ApiVersion(2, 1)]
    public class QoL : TerrariaPlugin
    {

        public override string Name => "QoL";
        public override Version Version => new Version(1, 1, 2);
        public override string Author => "Soofa";
        public override string Description => "Quality of life.";

        public QoL(Main game) : base(game) { }
        public static DateTime time = DateTime.UtcNow;
        public static List<int> QueenBeeIndexList = new();
        int[] DungeonWallIDs = { 7, 8, 9, 94, 95, 96, 97, 98, 99 };
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/QoLConfig.json");
        public static Config Config = new Config();
        private static bool OngoingVoteActive = false;
        private static int OngoingVoteType = -1;    // 0: Votekick, 1: Voteban
        private static string OngoingVoteAccountName = "";
        private static string OngoingVoteInGameName = "";
        private static string OngoingVoteIPAddress = "";
        private static string OngoingVoteUUID = "";
        private static int OngoingVoteCount = 0;
        private static Dictionary<string, bool> OngoingVoters = new Dictionary<string, bool>();
        public override void Initialize()
        {
            if (File.Exists(ConfigPath))
            {
                Config = Config.Read();
            }
            else
            {
                Config.Write();
            }

            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);

            if (Config.LockDungeonChestsTillSkeletron || Config.LockShadowChestsTillSkeletron) GetDataHandlers.ChestOpen += OnChestOpen;
            
            if (Config.QueenBeeRangeCheck)
            {
                ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
                ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            }

            if (Config.EnableLuckCommand)
            {
                Commands.ChatCommands.Add(new Command("qol.luck", LuckCmd, "luck")
                {
                    AllowServer = false,
                    HelpText = "Shows your luck."
                });
            }

            if (Config.EnableNewItemCommand)
            {
                Commands.ChatCommands.Remove(Commands.ChatCommands.Find(cmd => cmd.Name.Equals("item")));
                Commands.ChatCommands.Add(new Command(Permissions.item, ItemCmd, "item", "i")
                {
                    AllowServer = false,
                    HelpText = "Gives yourself an item."
                });
            }

            if (Config.EnableVotebanCommand)
            {
                Commands.ChatCommands.Add(new Command("qol.voteban", VotebanCmd, "voteban")
                {
                    AllowServer = true,
                    HelpText = "Starts a vote ban against a player"
                });
            }

            if (Config.EnableVotekickCommand)
            {
                Commands.ChatCommands.Add(new Command("qol.votekick", VotekickCmd, "votekick")
                {
                    AllowServer = true,
                    HelpText = "Starts a vote kick against a player"
                });
            }

            if (Config.EnableVotebanCommand || Config.EnableVotekickCommand)
            {
                Commands.ChatCommands.Add(new Command("qol.vote", VoteCmd, "vote")
                {
                    AllowServer = true,
                    HelpText = "Vote for current voting"
                });

                ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            }

            if (Config.EnableNameWhitelist) ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            if (Config.DisableQuickStack) ServerApi.Hooks.ItemForceIntoChest.Register(this, OnItemForceIntoChest);

            Commands.ChatCommands.Add(new Command("qol.execute", ExecuteCmd, "execute", "exe")
            {
                AllowServer = true,
                HelpText = "Executes multiple commands."
            });
        }

        private void OnItemForceIntoChest(ForceItemIntoChestEventArgs args)
        {
            args.Handled = true;
        }


        private void OnGamePostInitialize(EventArgs args)
        {
            Main.rand ??= new();
        }

        private void OnServerJoin(JoinEventArgs args)
        {
            if (!Config.WhitelistedNames.Contains(TShock.Players[args.Who].Name))
            {
                NetMessage.TrySendData((int)PacketTypes.Disconnect, args.Who, -1, Terraria.Localization.NetworkText.FromLiteral(TShock.Config.Settings.WhitelistKickReason));
            }
        }

        private void OnServerLeave(LeaveEventArgs args)
        {
            if (OngoingVoters.ContainsKey(TShock.Players[args.Who].Name))
            {
                OngoingVoteCount += OngoingVoters[TShock.Players[args.Who].Name] ? -1 : 1;
                OngoingVoters.Remove(TShock.Players[args.Who].Name);
            }
        }

        private void ExecuteCmd(CommandArgs args)
        {
            string[] cmds = string.Join(" ", args.Parameters).Split("&");

            foreach (string cmd in cmds)
            {
                Commands.HandleCommand(args.Player, cmd.Trim());
            }
        }

        private void VotekickCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Please specify a player name. (/votekick [name])");
                return;
            }

            if (OngoingVoteActive)
            {
                args.Player.SendErrorMessage("There is already an ongoing voting. Please wait till it ends.");
                return;
            }

            string targetName = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

            foreach (TSPlayer p in TShock.Players)
            {
                if (p == null || !p.Active)
                {
                    continue;
                }

                if (p.Name.Equals(targetName))
                {
                    OngoingVoteUUID = p.UUID;
                    OngoingVoteIPAddress = p.IP;
                    OngoingVoteAccountName = p.Account.Name;
                    OngoingVoteInGameName = p.Name;
                    OngoingVoteType = 0;
                    OngoingVoteCount = 1;
                    OngoingVoteActive = true;
                    break;
                }
                else if (p.Name.ToLower().StartsWith(targetName.ToLower()))
                {
                    OngoingVoteUUID = p.UUID;
                    OngoingVoteIPAddress = p.IP;
                    OngoingVoteAccountName = p.Account.Name;
                    OngoingVoteInGameName = p.Name;
                    OngoingVoteType = 0;
                    OngoingVoteCount = 1;
                    OngoingVoteActive = true;
                }
            }

            if (!OngoingVoteActive)
            {
                args.Player.SendErrorMessage($"{targetName} was not found.");
                return;
            }

            OngoingVoters.Add(args.Player.Name, true);
            TSPlayer.All.SendInfoMessage($"{args.Player.Name} has started votekick against {OngoingVoteInGameName}. Type \"/vote [y/n]\" to vote.");

            Task.Run(async () =>
            {
                await Task.Delay(60000);
                if (OngoingVoteCount >= TShock.Utils.GetActivePlayerCount() / 2 + 1)
                {
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/kick \"{OngoingVoteInGameName}\" \"Vote kicked.\"");
                    TSPlayer.All.SendInfoMessage($"{OngoingVoteInGameName} has been vote kicked.");
                }
                else
                {
                    TSPlayer.All.SendInfoMessage($"Not enough votes to kick {OngoingVoteInGameName}.");
                }

                OngoingVoters = new Dictionary<string, bool>();
                OngoingVoteActive = false;
            });
        }

        private void VotebanCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Please specify a player name. (/votekick [name])");
                return;
            }

            if (OngoingVoteActive)
            {
                args.Player.SendErrorMessage("There is already an ongoing vote. Please wait till it ends.");
                return;
            }

            string targetName = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));

            foreach (TSPlayer p in TShock.Players)
            {
                if (p == null || !p.Active)
                {
                    continue;
                }

                if (p.Name.Equals(targetName))
                {
                    OngoingVoteUUID = p.UUID;
                    OngoingVoteIPAddress = p.IP;
                    OngoingVoteAccountName = p.Account.Name;
                    OngoingVoteInGameName = p.Name;
                    OngoingVoteType = 1;
                    OngoingVoteCount = 1;
                    OngoingVoteActive = true;
                    break;
                }
                else if (p.Name.ToLower().StartsWith(targetName.ToLower()))
                {
                    OngoingVoteUUID = p.UUID;
                    OngoingVoteIPAddress = p.IP;
                    OngoingVoteAccountName = p.Account.Name;
                    OngoingVoteInGameName = p.Name;
                    OngoingVoteType = 1;
                    OngoingVoteCount = 1;
                    OngoingVoteActive = true;
                }
            }

            if (!OngoingVoteActive)
            {
                args.Player.SendErrorMessage($"{targetName} was not found.");
                return;
            }

            OngoingVoters.Add(args.Player.Name, true);
            TSPlayer.All.SendInfoMessage($"{args.Player.Name} has started voteban against {OngoingVoteInGameName}. ({OngoingVoteCount}/{TShock.Utils.GetActivePlayerCount() / 2 + 1})\nType \"/vote [y/n]\" to vote.");

            Task.Run(async () =>
            {
                await Task.Delay(60000);
                if (OngoingVoteCount >= TShock.Utils.GetActivePlayerCount() / 2 + 1)
                {
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/kick \"{OngoingVoteInGameName}\" \"Vote banned.\"");
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"acc:{OngoingVoteAccountName}\" \"Vote banned.\" {Config.VotebanTimeInMinutes}m -e");
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"ip:{OngoingVoteIPAddress}\" \"Vote banned.\" {Config.VotebanTimeInMinutes}m -e");
                    TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"uuid:{OngoingVoteUUID}\" \"Vote banned.\" {Config.VotebanTimeInMinutes}m -e");
                    TSPlayer.All.SendInfoMessage($"{OngoingVoteInGameName} has been vote banned for {Config.VotebanTimeInMinutes} mins.");
                }
                else
                {
                    TSPlayer.All.SendInfoMessage($"Not enough votes to kick {OngoingVoteInGameName}.");
                }

                OngoingVoters = new Dictionary<string, bool>();
                OngoingVoteActive = false;
            });
        }

        private void VoteCmd(CommandArgs args)
        {
            if (!OngoingVoteActive)
            {
                args.Player.SendErrorMessage("There is not an ongoing voting process");
                return;
            }

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Please specify y or n. (/vote [y/n])");
                return;
            }

            string typeText = OngoingVoteType == 0 ? "kicking" : "banning";

            if (args.Parameters[0].ToLower().StartsWith("y"))
            {
                if (OngoingVoters.ContainsKey(args.Player.Name))
                {
                    if (OngoingVoters[args.Player.Name])
                    {
                        args.Player.SendErrorMessage("You've already voted \"y\"");
                        return;
                    }
                    OngoingVoters[args.Player.Name] = true;
                }

                OngoingVoteCount++;
                TSPlayer.All.SendInfoMessage($"{args.Player.Name} has voted [c/22DD22:for] {typeText} {OngoingVoteInGameName}. ({OngoingVoteCount}/{TShock.Utils.GetActivePlayerCount() / 2 + 1})");
            }
            else if (args.Parameters[0].ToLower().StartsWith("n"))
            {
                if (OngoingVoters.ContainsKey(args.Player.Name))
                {
                    if (!OngoingVoters[args.Player.Name])
                    {
                        args.Player.SendErrorMessage("You've already voted \"n\"");
                        return;
                    }
                    OngoingVoters[args.Player.Name] = false;
                }

                OngoingVoteCount--;
                TSPlayer.All.SendInfoMessage($"{args.Player.Name} has voted [c/DD2222:against] {typeText} {OngoingVoteInGameName}. ({OngoingVoteCount}/{TShock.Utils.GetActivePlayerCount() / 2 + 1})");
            }
            else
            {
                args.Player.SendErrorMessage("Please specify a y or n. (/vote [y/n])");
            }
        }

        private static void ItemCmd(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax. Proper syntax: {0}item <item name/id> [item amount] [prefix id/name]", Commands.Specifier);
                return;
            }

            int amountParamIndex = -1;
            int itemAmount = 0;
            for (int i = 1; i < args.Parameters.Count; i++)
            {
                if (int.TryParse(args.Parameters[i], out itemAmount))
                {
                    amountParamIndex = i;
                    break;
                }
            }

            string itemNameOrId;
            if (amountParamIndex == -1)
            {
                itemNameOrId = string.Join(" ", args.Parameters);
            }
            else
            {
                itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));
            }

            Item item;
            List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemNameOrId);
            if (matchedItems.Count == 0)
            {
                args.Player.SendErrorMessage("Invalid item type!");
                return;
            }
            else if (matchedItems.Count > 1)
            {
                args.Player.SendMultipleMatchError(matchedItems.Select(i => $"[i:{i.netID}]{i.Name}({i.netID})"));
                return;
            }
            else
            {
                item = matchedItems[0];
            }
            if (item.type < 1 && item.type >= Terraria.ID.ItemID.Count)
            {
                args.Player.SendErrorMessage("The item type {0} is invalid.", itemNameOrId);
                return;
            }

            int prefixId = 0;
            if (amountParamIndex != -1 && args.Parameters.Count > amountParamIndex + 1)
            {
                string prefixidOrName = args.Parameters[amountParamIndex + 1];
                var prefixIds = TShock.Utils.GetPrefixByIdOrName(prefixidOrName);

                if (item.accessory && prefixIds.Contains(PrefixID.Quick))
                {
                    prefixIds.Remove(PrefixID.Quick);
                    prefixIds.Remove(PrefixID.Quick2);
                    prefixIds.Add(PrefixID.Quick2);
                }
                else if (!item.accessory && prefixIds.Contains(PrefixID.Quick))
                    prefixIds.Remove(PrefixID.Quick2);

                if (prefixIds.Count > 1)
                {
                    args.Player.SendMultipleMatchError(prefixIds.Select(p => p.ToString()));
                    return;
                }
                else if (prefixIds.Count == 0)
                {
                    args.Player.SendErrorMessage("No prefix matched \"{0}\".", prefixidOrName);
                    return;
                }
                else
                {
                    prefixId = prefixIds[0];
                }
            }

            if (args.Player.InventorySlotAvailable || (item.type > 70 && item.type < 75) || item.ammo > 0 || item.type == 58 || item.type == 184)
            {
                if (itemAmount == 0 || itemAmount > item.maxStack)
                    itemAmount = item.maxStack;

                if (args.Player.GiveItemCheck(item.type, EnglishLanguage.GetItemNameById(item.type), itemAmount, prefixId))
                {
                    item.prefix = (byte)prefixId;
                    args.Player.SendSuccessMessage((itemAmount < 2) ? "Gave {0} {1}." : "Gave {0} {1}s.", itemAmount, item.AffixName());
                }
                else
                {
                    args.Player.SendErrorMessage("You cannot spawn banned items.");
                }
            }
            else
            {
                args.Player.SendErrorMessage("Your inventory seems full.");
            }
        }

        private void OnChestOpen(object? sender, GetDataHandlers.ChestOpenEventArgs args)
        {
            if (Config.LockDungeonChestsTillSkeletron &&
                DungeonWallIDs.Contains(Main.tile[args.X, args.Y].wall) &&
                !NPC.downedBoss3)
            {
                args.Handled = true;
            }
            else if (Config.LockShadowChestsTillSkeletron &&
                Main.tile[args.X, args.Y].type == 21 &&
                Main.tile[args.X, args.Y].frameX / 36 == 3 &&
                !NPC.downedBoss3)
            {
                args.Handled = true;
            }
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            if (Main.npc[args.NpcId].netID == NPCID.QueenBee)
            {
                QueenBeeIndexList.Add(args.NpcId);
            }
        }

        private void OnGameUpdate(EventArgs args)
        {
            int[] indexesToRemove = { };
            foreach (int queenIndex in QueenBeeIndexList)
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
                QueenBeeIndexList.Remove(index);
            }
        }

        private void LuckCmd(CommandArgs args)
        {
            args.Player.SendInfoMessage($"Your luck is {args.Player.TPlayer.luck}.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
            }
            base.Dispose(disposing);
        }
    }
}