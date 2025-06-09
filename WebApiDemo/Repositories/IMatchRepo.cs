using WebApiDemo.Models;

namespace WebApiDemo.Repositories;

public interface IMatchRepo
{
    Task<MatchScore> GetMatchScore(int matchId);
    Task UpdateMatchScore(MatchScore matchScore);
} 