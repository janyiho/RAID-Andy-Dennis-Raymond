using Microsoft.AspNetCore.Mvc;
using WebApiDemo.Models;
using WebApiDemo.Repositories;

namespace WebApiDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private readonly IMatchRepo _matchRepo;

    public MatchController(IMatchRepo matchRepo)
    {
        _matchRepo = matchRepo;
    }

    [HttpPost("update-scores")]
    public async Task<string> UpdateMatchScores([FromQuery] int matchId, [FromQuery] MatchEvent matchEvent)
    {
        // First get the current match score
        var currentScore = await _matchRepo.GetMatchScore(matchId);
        var matchEventModel = new MatchEventModel(currentScore);

        // Handle the match event and get updated score
        var updatedScore = matchEventModel.HandleMatchEvent(matchEvent);
        
        // Save the updated score
        await _matchRepo.UpdateMatchScore(updatedScore);
        
        // Create new model with updated score for response
        var updatedEventModel = new MatchEventModel(updatedScore);
        
        return updatedEventModel.GetFormattedScore();
    }
} 