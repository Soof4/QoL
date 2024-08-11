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
        public override Version Version => new Version(1, 2, 4);
        public override string Author => "Soofa & Sors";
        public override string Description => "Quality of life.";

        public QoL(Main game) : base(game) => Instance = this;
        public static TerrariaPlugin? Instance;
        public static DateTime time = DateTime.UtcNow;
        public static List<int> QueenBeeIndexList = new();
        public static int[] DungeonWallIDs = { 7, 8, 9, 94, 95, 96, 97, 98, 99 };
        public static string ConfigPath = Path.Combine(TShock.SavePath + "/QoLConfig.json");
        public static Config Config = new Config();

        public override void Initialize()
        {
            Config = Config.Reload();
            Handlers.InitializeHandlers();
            Commands.InitializeCommands();
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
