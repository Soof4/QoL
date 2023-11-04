using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Localization;


namespace CheatShame {

    [ApiVersion(2, 1)]
    public class CheatShame : TerrariaPlugin {

        public override string Name => "QoL";
        public override Version Version => new Version(1, 0, 3);
        public override string Author => "Soofa";
        public override string Description => "Quality of life.";

        public CheatShame(Main game) : base(game) { }
        public static DateTime time = DateTime.UtcNow;
        public static List<int> QueenBeeIndexList = new();
        int[] DungeonWallIDs = {7, 8, 9, 94, 95, 96, 97, 98, 99};
        public override void Initialize() {
            GetDataHandlers.ChestOpen += OnChestOpen;
          
            ServerApi.Hooks.GameUpdate.Register(this, OnGameUpdate);
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);

            Commands.ChatCommands.Add(new Command("qol.luck", LuckCmd, "luck"));

            Commands.ChatCommands.Remove(Commands.ChatCommands.Find(cmd => cmd.Name.Equals("item")));
            Commands.ChatCommands.Add(new Command(Permissions.item, ItemCmd, "item", "i") {
                AllowServer = false,
                HelpText = "Gives yourself an item."
            });
        }
        
        private static void ItemCmd(CommandArgs args) {

            if (args.Parameters.Count < 1) {
                args.Player.SendErrorMessage("Invalid syntax. Proper syntax: {0}item <item name/id> [item amount] [prefix id/name]", Commands.Specifier);
                return;
            }

            int amountParamIndex = -1;
            int itemAmount = 0;
            for (int i = 1; i < args.Parameters.Count; i++) {
                if (int.TryParse(args.Parameters[i], out itemAmount)) {
                    amountParamIndex = i;
                    break;
                }
            }

            string itemNameOrId;
            if (amountParamIndex == -1) {
                itemNameOrId = string.Join(" ", args.Parameters);
            }
            else {
                itemNameOrId = string.Join(" ", args.Parameters.Take(amountParamIndex));
            }

            Item item;
            List<Item> matchedItems = TShock.Utils.GetItemByIdOrName(itemNameOrId);
            if (matchedItems.Count == 0) {
                args.Player.SendErrorMessage("Invalid item type!");
                return;
            }
            else if (matchedItems.Count > 1) {
                args.Player.SendMultipleMatchError(matchedItems.Select(i => $"[i:{i.netID}]{i.Name}({i.netID})"));
                return;
            }
            else {
                item = matchedItems[0];
            }
            if (item.type < 1 && item.type >= Terraria.ID.ItemID.Count) {
                args.Player.SendErrorMessage("The item type {0} is invalid.", itemNameOrId);
                return;
            }

            int prefixId = 0;
            if (amountParamIndex != -1 && args.Parameters.Count > amountParamIndex + 1) {
                string prefixidOrName = args.Parameters[amountParamIndex + 1];
                var prefixIds = TShock.Utils.GetPrefixByIdOrName(prefixidOrName);

                if (item.accessory && prefixIds.Contains(PrefixID.Quick)) {
                    prefixIds.Remove(PrefixID.Quick);
                    prefixIds.Remove(PrefixID.Quick2);
                    prefixIds.Add(PrefixID.Quick2);
                }
                else if (!item.accessory && prefixIds.Contains(PrefixID.Quick))
                    prefixIds.Remove(PrefixID.Quick2);

                if (prefixIds.Count > 1) {
                    args.Player.SendMultipleMatchError(prefixIds.Select(p => p.ToString()));
                    return;
                }
                else if (prefixIds.Count == 0) {
                    args.Player.SendErrorMessage("No prefix matched \"{0}\".", prefixidOrName);
                    return;
                }
                else {
                    prefixId = prefixIds[0];
                }
            }

            if (args.Player.InventorySlotAvailable || (item.type > 70 && item.type < 75) || item.ammo > 0 || item.type == 58 || item.type == 184) {
                if (itemAmount == 0 || itemAmount > item.maxStack)
                    itemAmount = item.maxStack;

                if (args.Player.GiveItemCheck(item.type, EnglishLanguage.GetItemNameById(item.type), itemAmount, prefixId)) {
                    item.prefix = (byte)prefixId;
                    args.Player.SendSuccessMessage((itemAmount < 2) ? "Gave {0} {1}." : "Gave {0} {1}s.", itemAmount, item.AffixName());
                }
                else {
                    args.Player.SendErrorMessage("You cannot spawn banned items.");
                }
            }
            else {
                args.Player.SendErrorMessage("Your inventory seems full.");
            }
        }

        private void OnChestOpen(object? sender, GetDataHandlers.ChestOpenEventArgs args) {
            if ((DungeonWallIDs.Contains(Main.tile[args.X, args.Y].wall) && !NPC.downedBoss3) ||  // dungeon chests
                Main.tile[args.X, args.Y].type == 21 && Main.tile[args.X, args.Y].frameX / 36 == 3 && !NPC.downedBoss3) {  // shadow chests
                args.Handled = true;
            }
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args) {
            if (Main.npc[args.NpcId].netID == NPCID.QueenBee) {
                QueenBeeIndexList.Add(args.NpcId);
            }
        }

        private void OnGameUpdate(EventArgs args) {
            int[] indexesToRemove = { };
            foreach (int queenIndex in QueenBeeIndexList) {
                NPC queenBee = Main.npc[queenIndex];

                bool isFarAway = true;
                for (int i = 0; i < TShock.Config.Settings.MaxSlots + TShock.Config.Settings.ReservedSlots; i++) {
                    TSPlayer? plr = TShock.Players[i];
                    if (plr != null && plr.Active && !plr.Dead && queenBee.position.WithinRange(plr.TPlayer.position, 16 * 450)) {
                        isFarAway = false;
                    }
                }

                if (isFarAway) {
                    queenBee.active = false;
                    queenBee.type = 0;
                    indexesToRemove.Append(queenIndex);
                    NetMessage.SendData((int)PacketTypes.NpcUpdate, number: queenIndex);
                }
            }

            foreach (int index in indexesToRemove) {
                QueenBeeIndexList.Remove(index);
            }
        }

        private void LuckCmd(CommandArgs args) {
            args.Player.SendInfoMessage($"Your luck is {args.Player.TPlayer.luck}.");
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnGameUpdate);
                ServerApi.Hooks.NpcSpawn.Deregister(this, OnNpcSpawn);
            }
            base.Dispose(disposing);
        }
    }
}