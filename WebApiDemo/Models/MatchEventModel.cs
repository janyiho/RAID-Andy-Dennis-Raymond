using WebApiDemo.Exceptions;
using WebApiDemo.Models;

namespace WebApiDemo.Models;

public class MatchEventModel
{
    private readonly string _scores;
    private readonly bool _isSecondHalf;
    private readonly string _firstHalf;
    private readonly string _secondHalf;

    public int MatchId { get; }

    public MatchEventModel(MatchScore matchScore)
    {
        MatchId = matchScore.MatchId;
        _scores = matchScore.Scores;
        _isSecondHalf = _scores.Contains(";");
        var parts = _scores.Split(';');
        _firstHalf = parts[0];
        _secondHalf = parts.Length > 1 ? parts[1] : "";
    }

    public string GetFormattedScore()
    {
        var homeGoals = _scores.Count(c => c == 'H');
        var awayGoals = _scores.Count(c => c == 'A');
        return $"{homeGoals}-{awayGoals} ({(_isSecondHalf ? "Second Half" : "First Half")})";
    }

    private static string CancelGoal(string score, char team)
    {
        if (string.IsNullOrEmpty(score) || !score.EndsWith(team))
            throw new InvalidMatchEventException($"Cannot cancel {(team == 'H' ? "home" : "away")} goal: last score is not from {(team == 'H' ? "home" : "away")} team");
        return score.Remove(score.Length - 1);
    }

    private static string AddGoal(string score, char team) => score + team;

    private (string firstHalf, string secondHalf) HandleHomeGoal(string firstHalf, string secondHalf)
    {
        if (_isSecondHalf) secondHalf = AddGoal(secondHalf, 'H');
        else firstHalf = AddGoal(firstHalf, 'H');
        return (firstHalf, secondHalf);
    }

    private (string firstHalf, string secondHalf) HandleAwayGoal(string firstHalf, string secondHalf)
    {
        if (_isSecondHalf) secondHalf = AddGoal(secondHalf, 'A');
        else firstHalf = AddGoal(firstHalf, 'A');
        return (firstHalf, secondHalf);
    }

    private (string firstHalf, string secondHalf) HandleCancelHomeGoal(string firstHalf, string secondHalf)
    {
        if (_isSecondHalf && !string.IsNullOrEmpty(secondHalf))
            secondHalf = CancelGoal(secondHalf, 'H');
        else
            firstHalf = CancelGoal(firstHalf, 'H');
        return (firstHalf, secondHalf);
    }

    private (string firstHalf, string secondHalf) HandleCancelAwayGoal(string firstHalf, string secondHalf)
    {
        if (_isSecondHalf && !string.IsNullOrEmpty(secondHalf))
            secondHalf = CancelGoal(secondHalf, 'A');
        else
            firstHalf = CancelGoal(firstHalf, 'A');
        return (firstHalf, secondHalf);
    }

    private (string firstHalf, string secondHalf) HandleNextPeriod(string firstHalf, string secondHalf)
    {
        if (!_isSecondHalf) firstHalf += ";";
        return (firstHalf, secondHalf);
    }

    public MatchScore HandleMatchEvent(MatchEvent matchEvent)
    {
        var firstHalf = _firstHalf;
        var secondHalf = _secondHalf;

        var (updatedFirstHalf, updatedSecondHalf) = matchEvent switch
        {
            MatchEvent.HomeGoal => HandleHomeGoal(firstHalf, secondHalf),
            MatchEvent.AwayGoal => HandleAwayGoal(firstHalf, secondHalf),
            MatchEvent.CancelHomeGoal => HandleCancelHomeGoal(firstHalf, secondHalf),
            MatchEvent.CancelAwayGoal => HandleCancelAwayGoal(firstHalf, secondHalf),
            MatchEvent.NextPeriod => HandleNextPeriod(firstHalf, secondHalf),
            _ => (firstHalf, secondHalf)
        };

        return new MatchScore { MatchId = MatchId, Scores = _isSecondHalf ? $"{updatedFirstHalf};{updatedSecondHalf}" : updatedFirstHalf };
    }
} 