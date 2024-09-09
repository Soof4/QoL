using QoL.Enums;
using TShockAPI;

namespace QoL;

public static class VoteService
{
    private static Vote? _curVote = null;
    private static int _requiredPoint => TShock.Utils.GetActivePlayerCount() / 2 + 1;
    private static List<int> _yesVoters = new();
    private static List<int> _noVoters = new();

    private static void Reset()
    {
        _curVote = null;
        _yesVoters = new();
        _noVoters = new();
    }

    public static void StartVote(TSPlayer starter, TSPlayer target, VoteType voteType)
    {
        if (_curVote != null)
        {
            starter.SendErrorMessage("There is an ongoing voting. Please wait till it ends.");
            return;
        }

        _curVote = new Vote(voteType, starter, target);
        _curVote.Announce();
        TryAddVote(starter, true);

        Task.Run(async () =>
        {
            await Task.Delay(QoL.Config.VoteDurationInMinutes * 60000);
            if (_curVote != null)
            {
                _curVote.End(_requiredPoint);
                Reset();
            }
        });
    }

    public static void TryAddVote(TSPlayer player, bool yes)
    {
        if (_curVote == null)
        {
            return;
        }
        else if (yes)
        {
            if (_yesVoters.Contains(player.Index))
            {
                player.SendErrorMessage("You've already voted yes.");
                return;
            }
            else if (_noVoters.Contains(player.Index))
            {
                _noVoters.Remove(player.Index);
                _curVote.Point++;
            }

            _curVote.Point++;
            _yesVoters.Add(player.Index);
            string typeText = _curVote.VoteType == VoteType.Kick ? "kicking" : "banning";
            TSPlayer.All.SendInfoMessage($"{player.Name} has voted [c/22DD22:for] {typeText} {_curVote.Target.Name}. ({_curVote.Point}/{_requiredPoint})");
        }
        else if (!yes)
        {
            if (_noVoters.Contains(player.Index))
            {
                player.SendErrorMessage("You've already voted no.");
                return;
            }
            else if (_yesVoters.Contains(player.Index))
            {
                _yesVoters.Remove(player.Index);
                _curVote.Point--;
            }

            _curVote.Point--;
            _noVoters.Add(player.Index);
            string typeText = _curVote.VoteType == VoteType.Kick ? "kicking" : "banning";
            TSPlayer.All.SendInfoMessage($"{player.Name} has voted [c/DD2222:against] {typeText} {_curVote.Target.Name}. ({_curVote.Point}/{_requiredPoint})");
        }

        int requiredPoint = _requiredPoint;
        if (_curVote.Point >= requiredPoint)
        {
            _curVote.End(requiredPoint);
            Reset();
        }
    }

    public static void TryRemoveVote(int playerIndex)
    {
        if (_curVote == null)
        {
            return;
        }

        if (_yesVoters.Contains(playerIndex))
        {
            _yesVoters.Remove(playerIndex);
            _curVote.Point--;
        }
        else if (_noVoters.Contains(playerIndex))
        {
            _noVoters.Remove(playerIndex);
            _curVote.Point++;
        }

        int requiredPoint = _requiredPoint;
        if (_curVote.Point >= requiredPoint)
        {
            _curVote.End(requiredPoint);
            Reset();
        }
    }
}