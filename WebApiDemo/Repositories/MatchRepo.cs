using WebApiDemo.Models;

namespace WebApiDemo.Repositories;

public class MatchRepo : IMatchRepo
{
    private readonly Dictionary<int, string> _matchScores = new();

    public async Task<MatchScore> GetMatchScore(int matchId)
    {
        await Task.Delay(100); // Simulate some async work

        if (!_matchScores.ContainsKey(matchId))
        {
            return new MatchScore
            {
                MatchId = matchId,
                Scores = string.Empty
            };
        }

        return new MatchScore
        {
            MatchId = matchId,
            Scores = _matchScores[matchId]
        };
    }

    public async Task UpdateMatchScore(MatchScore matchScore)
    {
        await Task.Delay(100); // Simulate some async work
        _matchScores[matchScore.MatchId] = matchScore.Scores;
    }
} 