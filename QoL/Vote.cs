using QoL.Enums;
using TShockAPI;

namespace QoL;

public class Vote
{
    public VoteType VoteType { get; set; }
    public TSPlayer Starter { get; set; }
    public TSPlayer Target { get; set; }
    public int Point { get; set; } = 0;


    public Vote(VoteType voteType, TSPlayer starter, TSPlayer target)
    {
        VoteType = voteType;
        Starter = starter;
        Target = target;
    }


    public void End(int requiredPoint)
    {
        if (Point < requiredPoint)
        {
            TSPlayer.All.SendErrorMessage($"Vote against {Target.Name} has failed.");
            return;
        }

        switch (VoteType)
        {
            case VoteType.Kick:
                Target.Kick("You've been votekicked!");
                TSPlayer.All.SendInfoMessage($"{Target.Name} has been votekicked!");
                break;
            case VoteType.Ban:
                TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"acc:{Target.Account.Name}\" \"You've been votebanned!\" {QoL.Config.VotebanTimeInMinutes}m -e");
                TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"uuid:{Target.UUID}\" \"You've been votebanned!\" {QoL.Config.VotebanTimeInMinutes}m -e");
                TShockAPI.Commands.HandleCommand(TSPlayer.Server, $"/ban add \"ip:{Target.IP}\" \"You've been votebanned!\" {QoL.Config.VotebanTimeInMinutes}m -e");
                TSPlayer.All.SendInfoMessage($"{Target.Name} has been votebanned!");
                break;
        }
    }

    public void Announce()
    {
        switch (VoteType)
        {
            case VoteType.Kick:
                TSPlayer.All.SendInfoMessage($"{Starter.Name} has started votekick against {Target.Account.Name}. Type \"/vote <y/n>\" to vote.");
                break;
            case VoteType.Ban:
                TSPlayer.All.SendInfoMessage($"{Starter.Name} has started voteban against {Target.Account.Name}. Type \"/vote <y/n>\" to vote.");
                break;
        }
    }
}