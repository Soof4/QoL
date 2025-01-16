using QoL.Enums;
using Terraria;
using Terraria.ID;
using TShockAPI;
using TShockAPI.Localization;

namespace QoL;

public static class Commands
{
    public static int[] PlatformTileIDs = new int[] { 19, 427, 435, 436, 437, 438, 439 };

    public static void InitializeCommands()
    {

        Utils.AddCommand(
            permissions: "qol.luck",
            cmd: LuckCmd,
            names: "luck",
            allowServer: false,
            helpText: "Shows your luck. Usage: /luck"
        );

        TShockAPI.Commands.ChatCommands.Remove(TShockAPI.Commands.ChatCommands.Find(cmd => cmd.Name.Equals("item")));

        Utils.AddCommand(
            permissions: Permissions.item,
            cmd: ItemCmd,
            names: new string[] { "item", "i" },
            allowServer: false,
            helpText: "Gives yourself an item."
        );

        Utils.AddCommand(
            permissions: "qol.voteban",
            cmd: VotebanCmd,
            names: "voteban",
            allowServer: true,
            helpText: "Starts a vote ban against a player. Usage: /voteban <name>"
        );

        Utils.AddCommand(
            permissions: "qol.votekick",
            cmd: VotekickCmd,
            names: "votekick",
            allowServer: true,
            helpText: "Starts a vote kick against a player. Usage: /votekick <name>"
        );

        Utils.AddCommand(
            permissions: "qol.vote",
            cmd: VoteCmd,
            names: "vote",
            allowServer: true,
            helpText: "Vote for current voting. Usage: /vote <y/n>"
        );

        Utils.AddCommand(
            permissions: "qol.execute",
            cmd: ExecuteCmd,
            names: new string[] { "execute", "exe" },
            allowServer: true,
            helpText: "Executes multiple commands. Usage: /execute [cmd1] & [cmd2] & [cmd3] ...\n" +
                "Example: /execute wind 1 & worlevent sandstorm & broadcast \"Hello world!\""
        );

        Utils.AddCommand(
            permissions: "qol.iteminfo",
            cmd: InfoCmd,
            names: new string[] { "iteminfo", "ii" },
            allowServer: true,
            helpText: "Shows item info. Usage: /iteminfo <item name>"
        );

        Utils.AddCommand(
            permissions: "qol.builder",
            cmd: BuilderCmd,
            names: "builder",
            allowServer: false,
            helpText: "Instantiates the journey mode menu."
        );

        Utils.AddCommand(
            permissions: "qol.tpn",
            cmd: TpnCmd,
            names: "tpn",
            allowServer: false,
            helpText: "Teleports to town NPCs only."
        );

        Utils.AddCommand(
            permissions: "qol.banmobile",
            cmd: BanMobileCmd,
            names: new string[] { "banmobile", "banm" },
            allowServer: true,
            helpText: "A wrapper command of /ban for mobile users."
        );
    }

    public static void ReloadCommands()
    {
        foreach (var cap in QoL.Config.CommandAliases)
        {
            foreach (var cmd in TShockAPI.Commands.ChatCommands)
            {
                if (cmd.Names.Contains(cap.Key) && !cmd.Names.Contains(cap.Value))
                {
                    cmd.Names.Add(cap.Value);
                }
            }
        }
    }

    private static void BuilderCmd(CommandArgs args)
    {
        BitsByte plrdifficulty = new BitsByte();
        BitsByte misc = new BitsByte();
        BitsByte pernamentupgr = new BitsByte();

        plrdifficulty[0] = plrdifficulty[1] = false;
        plrdifficulty[2] = args.TPlayer.extraAccessory;
        plrdifficulty[3] = true; //creative mode, tshock doc did not update this (and a lot more other things) lead to falsely transfered information
        misc[0] = args.TPlayer.UsingBiomeTorches;
        misc[1] = args.TPlayer.happyFunTorchTime;
        misc[2] = args.TPlayer.unlockedSuperCart;
        misc[3] = args.TPlayer.enabledSuperCart;
        pernamentupgr[0] = args.TPlayer.usedAegisCrystal;
        pernamentupgr[1] = args.TPlayer.usedAegisFruit;
        pernamentupgr[2] = args.TPlayer.usedArcaneCrystal;
        pernamentupgr[3] = args.TPlayer.usedGalaxyPearl;
        pernamentupgr[4] = args.TPlayer.usedGummyWorm;
        pernamentupgr[5] = args.TPlayer.usedAmbrosia;
        pernamentupgr[6] = args.TPlayer.ateArtisanBread;

        byte[] playerInfo = new PacketFactory()
                .SetPacketType((short)PacketTypes.PlayerInfo)
                .PackByte((byte)args.Player.Index)
                .PackByte((byte)args.TPlayer.skinVariant)
                .PackByte((byte)args.TPlayer.hair)
                .PackString(args.TPlayer.name)
                .PackByte(args.TPlayer.hairDye)
                .PackAccessoryVisibility(args.TPlayer.hideVisibleAccessory)
                .PackByte(args.TPlayer.hideMisc)
                .PackColor(args.TPlayer.hairColor)
                .PackColor(args.TPlayer.skinColor)
                .PackColor(args.TPlayer.eyeColor)
                .PackColor(args.TPlayer.shirtColor)
                .PackColor(args.TPlayer.underShirtColor)
                .PackColor(args.TPlayer.pantsColor)
                .PackColor(args.TPlayer.shoeColor)
                .PackByte(plrdifficulty)
                .PackByte(misc)
                .PackByte(pernamentupgr)
                .GetByteData();

        args.Player.SendRawData(playerInfo);

        // Unlock everything
        for (short i = 0; i < ItemID.Count; i++)
        {
            //old method also works!
            byte[] creativeUnlock = new PacketFactory(writeExtraOffsetForNetModule: true)
                    .SetPacketType((short)PacketTypes.LoadNetModule)
                    .SetNetModuleType(5)
                    .PackInt16(i)
                    .PackUInt16(9999)
                    .GetByteData();

            args.Player.SendRawData(creativeUnlock);
        }
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

        Item _item = items.First();

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

    private static void ExecuteCmd(CommandArgs args)
    {
        string[] cmds = string.Join(' ', args.Parameters).Split("&");

        // Check for any disallowed commands (I feel like this part can be optimized)
        foreach (string cmd in cmds)
        {
            // Split the cmd name from its parameters
            string cmdName = cmd.Split(' ')[0];
            cmdName = cmdName[1..];

            // Get aliases
            List<string> cmdNames = new List<string>() { cmdName };

            foreach (var tc in TShockAPI.Commands.ChatCommands)
            {
                if (tc.Names.Contains(cmdNames[0]))
                {
                    cmdNames.AddRange(tc.Names);
                    cmdNames.RemoveAt(0);
                }
            }

            // Check if any commands match
            if (cmdNames.Any(c => QoL.Config.DisallowedExecuteCommands.Contains(c)))
            {
                args.Player.SendErrorMessage("Your execute command contains a disallowed command.");
                return;
            }
        }

        // Execution
        foreach (string cmd in cmds)
        {
            TShockAPI.Commands.HandleCommand(args.Player, cmd.Trim());
        }
    }

    private static void VotekickCmd(CommandArgs args)
    {
        if (args.Parameters.Count < 1)
        {
            args.Player.SendErrorMessage("Please specify a player name. Usage: /votekick <name>");
            return;
        }

        string targetName = string.Join(" ", args.Parameters.GetRange(0, args.Parameters.Count));

        List<TSPlayer> matchedPlayers = TSPlayer.FindByNameOrID(targetName);

        if (matchedPlayers.Count == 0)
        {
            args.Player.SendErrorMessage("Player not found.");
            return;
        }

        if (matchedPlayers.Count > 1)
        {
            args.Player.SendErrorMessage("Found multiple players. Please be more specific.");
            return;
        }

        VoteService.StartVote(args.Player, matchedPlayers.First(), VoteType.Kick);
    }

    private static void VotebanCmd(CommandArgs args)
    {
        if (args.Parameters.Count < 1)
        {
            args.Player.SendErrorMessage("Please specify a player name. Usage: /voteban <name>");
            return;
        }

        string targetName = string.Join(" ", args.Parameters.GetRange(0, args.Parameters.Count));

        List<TSPlayer> matchedPlayers = TSPlayer.FindByNameOrID(targetName);

        if (matchedPlayers.Count == 0)
        {
            args.Player.SendErrorMessage("Player not found.");
            return;
        }

        if (matchedPlayers.Count > 1)
        {
            args.Player.SendErrorMessage("Found multiple players. Please be more specific.");
            return;
        }

        VoteService.StartVote(args.Player, matchedPlayers.First(), VoteType.Ban);
    }

    private static void VoteCmd(CommandArgs args)
    {
        if (args.Parameters.Count < 1)
        {
            args.Player.SendErrorMessage("Please specify a y or n. (/vote [y/n])");
            return;
        }

        switch (args.Parameters[0].ToLower()[0])
        {
            case 'y':
                VoteService.TryAddVote(args.Player, true);
                break;
            case 'n':
                VoteService.TryAddVote(args.Player, false);
                break;
            default:
                args.Player.SendErrorMessage("Please specify a y or n. (/vote [y/n])");
                break;
        }
    }

    private static void ItemCmd(CommandArgs args)
    {
        if (args.Parameters.Count < 1)
        {
            args.Player.SendErrorMessage("Invalid syntax. Proper syntax: {0}item <item name/id> [item amount] [prefix id/name]", TShockAPI.Commands.Specifier);
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

    private static void LuckCmd(CommandArgs args)
    {
        args.Player.SendInfoMessage($"Your luck is {args.Player.TPlayer.luck}.");
    }

    private static void TpnCmd(CommandArgs args)
    {
        if (args.Parameters.Count < 1)
        {
            args.Player.SendErrorMessage("Invalid syntax. Proper syntax: {0}tpn <NPC>.", TShockAPI.Commands.Specifier);
            return;
        }

        var npcStr = string.Join(" ", args.Parameters);
        var matches = new List<NPC>();
        foreach (var npc in Main.npc.Where(npc => npc.active))
        {
            var englishName = EnglishLanguage.GetNpcNameById(npc.netID);

            if (string.Equals(npc.FullName, npcStr, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(englishName, npcStr, StringComparison.InvariantCultureIgnoreCase))
            {
                matches = new List<NPC> { npc };
                break;
            }
            if (npc.FullName.ToLowerInvariant().StartsWith(npcStr.ToLowerInvariant()) ||
                englishName?.StartsWith(npcStr, StringComparison.InvariantCultureIgnoreCase) == true)
                matches.Add(npc);
        }

        // Filter non-town NPCs.
        matches = matches.Where(n => Utils.TownNPCs.Contains(n.netID)).ToList();

        if (matches.Count > 1)
        {
            args.Player.SendMultipleMatchError(matches.Select(n => $"{n.FullName}({n.whoAmI})"));
            return;
        }
        if (matches.Count == 0)
        {
            args.Player.SendErrorMessage("Invalid destination NPC.");
            return;
        }

        var target = matches[0];
        args.Player.Teleport(target.position.X, target.position.Y);
        args.Player.SendSuccessMessage("Teleported to the '{0}'.", target.FullName);
    }

    private static void BanMobileCmd(CommandArgs args)
    {
        string cmdSpecifier = args.Silent ? TShock.Config.Settings.CommandSilentSpecifier : TShock.Config.Settings.CommandSpecifier;
        TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"{cmdSpecifier}ban {args.Parameters[0]} \"{args.Parameters[1]}\" {string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2))}");
        args.Player.SendInfoMessage("Ban command has been executed, if the player is not kicked then there must be an error in the written command.");
    }
}