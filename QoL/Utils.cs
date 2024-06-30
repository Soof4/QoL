using Terraria;

namespace QoL
{
    public static class Utils
    {

        public static string GetDamageTypeText(this Item item)
        {
            if (item.melee) return "melee";
            if (item.magic) return "magic";
            if (item.ranged) return "ranged";
            if (item.summon) return "summon";

            return "unknown";
        }

        public static int GetRealRarity(this Item item)
        {
            if (item.expertOnly)
            {
                return -13;
            }
            else if (item.expert && !item.vanity)
            {
                return -12;
            }

            return item.OriginalRarity;
        }

        public static string GetRarityColorText(int rarity)
        {
            switch (rarity)
            {
                case 0:
                    return "[c/ffffff:White]";
                case 1:
                    return "[c/9696ff:Blue]";
                case 2:
                    return "[c/96ff96:Green]";
                case 3:
                    return "[c/ffc896:Orange]";
                case 4:
                    return "[c/ff9696:Light Red]";
                case 5:
                    return "[c/ff96ff:Pink]";
                case 6:
                    return "[c/d2a0ff:Light Purple]";
                case 7:
                    return "[c/96ff0a:Lime]";
                case 8:
                    return "[c/ffff0a:Yellow]";
                case 9:
                    return "[c/05c8ff:Cyan]";
                case 10:
                    return "[c/ff2864:Red]";
                case 11:
                    return "[c/b428ff:Purple]";
                case -11:
                    return "[c/ffaf00:Amber]";
                case -12:
                    return "[c/ff0000:R][c/ff7f00:a][c/ffff00:i][c/00ff00:n][c/0000ff:b][c/4b0082:o][c/cf00ff:w]";
                case -13:
                    return "[c/e50000:F][c/ea3800:i][c/ee5400:e][c/f26a00:r][c/f79100:y] [c/fcb51b:R][c/fdc52e:e][c/ffd641:d]";
                default:
                    return "[c/828282:Gray]";
            }
        }
    }
}