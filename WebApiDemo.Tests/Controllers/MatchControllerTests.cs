using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebApiDemo.Controllers;
using WebApiDemo.Models;
using WebApiDemo.Repositories;
using Xunit;

namespace WebApiDemo.Tests.Controllers;

public class MatchControllerTests
{
    private readonly IMatchRepo _matchRepo;
    private readonly MatchController _controller;

    public MatchControllerTests()
    {
        _matchRepo = Substitute.For<IMatchRepo>();
        _controller = new MatchController(_matchRepo);
    }

    [Fact]
    public async Task UpdateMatchScores_WhenHomeGoalInFirstHalf_ReturnsCorrectScore()
    {
        // Arrange
        var matchId = 91;
        var currentScore = new MatchScore { MatchId = matchId, Scores = "" };

        _matchRepo.GetMatchScore(matchId).Returns(currentScore);

        // Act
        var result = await _controller.UpdateMatchScores(matchId, MatchEvent.HomeGoal);

        // Assert
        Assert.Equal("1-0 (First Half)", result);
        await _matchRepo.Received(1).UpdateMatchScore(Arg.Any<MatchScore>());
    }

    [Fact]
    public async Task UpdateMatchScores_WhenAwayGoalInSecondHalf_ReturnsCorrectScore()
    {
        // Arrange
        var matchId = 91;
        var currentScore = new MatchScore { MatchId = matchId, Scores = "H;" };

        _matchRepo.GetMatchScore(matchId).Returns(currentScore);

        // Act
        var result = await _controller.UpdateMatchScores(matchId, MatchEvent.AwayGoal);

        // Assert
        Assert.Equal("1-1 (Second Half)", result);
        await _matchRepo.Received(1).UpdateMatchScore(Arg.Any<MatchScore>());
    }

    [Fact]
    public async Task UpdateMatchScores_WhenCancelHomeGoal_ReturnsCorrectScore()
    {
        // Arrange
        var matchId = 91;
        var currentScore = new MatchScore { MatchId = matchId, Scores = "HH" };

        _matchRepo.GetMatchScore(matchId).Returns(currentScore);

        // Act
        var result = await _controller.UpdateMatchScores(matchId, MatchEvent.CancelHomeGoal);

        // Assert
        Assert.Equal("1-0 (First Half)", result);
        await _matchRepo.Received(1).UpdateMatchScore(Arg.Any<MatchScore>());
    }

    [Fact]
    public async Task UpdateMatchScores_WhenNextPeriod_ReturnsCorrectScore()
    {
        // Arrange
        var matchId = 91;
        var currentScore = new MatchScore { MatchId = matchId, Scores = "HA" };

        _matchRepo.GetMatchScore(matchId).Returns(currentScore);

        // Act
        var result = await _controller.UpdateMatchScores(matchId, MatchEvent.NextPeriod);

        // Assert
        Assert.Equal("1-1 (Second Half)", result);
        await _matchRepo.Received(1).UpdateMatchScore(Arg.Any<MatchScore>());
    }
} 