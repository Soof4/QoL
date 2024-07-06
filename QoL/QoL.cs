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
        public override Version Version => new Version(1, 2, 0);
        public override string Author => "Soofa";
        public override string Description => "Quality of life.";

        public QoL(Main game) : base(game) => Instance = this;
        public static TerrariaPlugin? Instance;
        public static DateTime time = DateTime.UtcNow;
        public static List<int> QueenBeeIndexList = new();
        public static int[] DungeonWallIDs = { 7, 8, 9, 94, 95, 96, 97, 98, 99 };
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/QoLConfig.json");
        public static Config Config = new Config();
        public static bool OngoingVoteActive = false;
        public static int OngoingVoteType = -1;    // 0: Votekick, 1: Voteban
        public static string OngoingVoteAccountName = "";
        public static string OngoingVoteInGameName = "";
        public static string OngoingVoteIPAddress = "";
        public static string OngoingVoteUUID = "";
        public static int OngoingVoteCount = 0;
        public static Dictionary<string, bool> OngoingVoters = new Dictionary<string, bool>();
        public static int[] PlatformTileIDs = new int[] { 19, 427, 435, 436, 437, 438, 439 };

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

            Handlers.InitializeHandlers();

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

                ServerApi.Hooks.ServerLeave.Register(this, Handlers.OnServerLeave);
            }

            Commands.ChatCommands.Add(new Command("qol.execute", ExecuteCmd, "execute", "exe")
            {
                AllowServer = true,
                HelpText = "Executes multiple commands."
            });

            Commands.ChatCommands.Add(new Command("qol.iteminfo", InfoCmd, "iteminfo", "ii")
            {
                AllowServer = true,
                HelpText = "Shows item info. Usage: /iteminfo <item name>"
            });
        }

        private static void InfoCmd(CommandArgs args)
        {
            string itemName = string.Join(" ", args.Parameters);

            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Please specify an item name.");
                return;
            }

            List<Item> items = TShock.Utils.GetItemByIdOrName(itemName);

            if (items.Count < 1)
            {
                args.Player.SendErrorMessage($"{itemName} not found.");
                return;
            }

            Item _item = ContentSamples.ItemsByType[items[0].type];

            string msg = $"Info about {_item.Name} ([i:{_item.type}]):";

            msg += $"\nRarity: {Utils.GetRarityColorText(_item.OriginalRarity)}";
            if (_item.damage > 0) msg += $"\nDamage: {_item.damage} ({_item.GetDamageTypeText()})";
            if (_item.crit != 0) msg += $"\nCrit chance: {_item.crit}";
            if (_item.defense != 0) msg += $"\nDefense: {_item.defense}";
            if (_item.pick != 0) msg += $"\nPickaxe power: {_item.pick}%";
            if (_item.axe != 0) msg += $"\nAxe power: {_item.axe}%";
            if (_item.hammer != 0) msg += $"\nHammer power: {_item.hammer}%";
            if (_item.bait != 0) msg += $"\nBait power: {_item.bait}%";

            msg += "\nRecipes:";
            int recipeCount = 0;
            int type = _item.type;

            for (int i = 0; i < Recipe.maxRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe.createItem.type == 0)
                {
                    break;
                }

                if (recipe.createItem.type != _item.type)
                {
                    continue;
                }

                recipeCount++;
                string rec = $"\n{recipeCount}. ";

                for (int j = 0; j < Recipe.maxRequirements; j++)
                {
                    Item item = recipe.requiredItem[j];

                    if (item.type == 0)
                    {
                        break;
                    }

                    if (Main.guideItem.IsTheSameAs(item) ||
                        recipe.useWood(type, item.type) ||
                        recipe.useSand(type, item.type) ||
                        recipe.useIronBar(type, item.type) ||
                        recipe.useFragment(type, item.type) ||
                        recipe.AcceptedByItemGroups(type, item.type) ||
                        recipe.usePressurePlate(type, item.type))
                    {
                        Main.availableRecipe[Main.numAvailableRecipes] = i;
                        Main.numAvailableRecipes++;
                        break;
                    }

                    rec += $"[i/s{item.stack}:{item.type}]";
                }

                msg += rec;
            }

            if (recipeCount == 0)
            {
                msg += "\nUncraftable";
            }

            args.Player.SendInfoMessage(msg);
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

        private void LuckCmd(CommandArgs args)
        {
            args.Player.SendInfoMessage($"Your luck is {args.Player.TPlayer.luck}.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Handlers.DisposeHandlers();
            }
            base.Dispose(disposing);
        }
    }
}
