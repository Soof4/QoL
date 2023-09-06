using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;


namespace CheatShame {

    [ApiVersion(2, 1)]
    public class CheatShame : TerrariaPlugin {

        public override string Name => "QoL";
        public override Version Version => new Version(1, 0, 0);
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
        }

        private void OnChestOpen(object? sender, GetDataHandlers.ChestOpenEventArgs args) {
            if (DungeonWallIDs.Contains(Main.tile[args.X, args.Y].wall) && !NPC.downedBoss3) {
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
            args.Player.SendInfoMessage($"Your luck is {args.Player.TPlayer.luck}");
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