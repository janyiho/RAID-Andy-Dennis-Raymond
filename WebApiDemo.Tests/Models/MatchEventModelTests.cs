using WebApiDemo.Exceptions;
using WebApiDemo.Models;
using Xunit;

namespace WebApiDemo.Tests.Models;

public class MatchEventModelTests
{
    [Fact]
    public void Constructor_WithEmptyScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(91, model.MatchId);
        Assert.Equal("0-0 (First Half)", model.GetFormattedScore());
    }

    [Fact]
    public void Constructor_WithFirstHalfScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HHA" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(91, model.MatchId);
        Assert.Equal("2-1 (First Half)", model.GetFormattedScore());
    }

    [Fact]
    public void Constructor_WithSecondHalfScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HA;AH" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(91, model.MatchId);
        Assert.Equal("2-2 (Second Half)", model.GetFormattedScore());
    }

    [Fact]
    public void GetFormattedScore_FirstHalf_ReturnsCorrectFormat()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HH" };
        var model = new MatchEventModel(matchScore);

        // Act
        var result = model.GetFormattedScore();

        // Assert
        Assert.Equal("2-0 (First Half)", result);
    }

    [Fact]
    public void GetFormattedScore_SecondHalf_ReturnsCorrectFormat()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HA;A" };
        var model = new MatchEventModel(matchScore);

        // Act
        var result = model.GetFormattedScore();

        // Assert
        Assert.Equal("1-2 (Second Half)", result);
    }

    [Theory]
    [InlineData(MatchEvent.HomeGoal, "", "H", "1-0")]
    [InlineData(MatchEvent.AwayGoal, "H", "HA", "1-1")]
    [InlineData(MatchEvent.CancelHomeGoal, "HH", "H", "1-0")]
    [InlineData(MatchEvent.CancelAwayGoal, "HA", "H", "1-0")]
    [InlineData(MatchEvent.NextPeriod, "HA", "HA;", "1-1")]
    public void HandleMatchEvent_WithDifferentEvents_ReturnsCorrectScore(
        MatchEvent matchEvent, 
        string initialScore, 
        string expectedScore, 
        string expectedTotal)
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = initialScore };
        var model = new MatchEventModel(matchScore);

        // Act
        var result = model.HandleMatchEvent(matchEvent);

        // Assert
        Assert.Equal(91, result.MatchId);
        Assert.Equal(expectedScore, result.Scores);
        
        // Verify the score calculation
        var updatedModel = new MatchEventModel(result);
        var period = updatedModel.GetFormattedScore().Contains("Second Half") ? "Second Half" : "First Half";
        Assert.Equal($"{expectedTotal} ({period})", updatedModel.GetFormattedScore());
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelHomeGoalWithNoHomeGoals_ThrowsException()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "A" };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(MatchEvent.CancelHomeGoal));
        Assert.Equal("Cannot cancel home goal: last score is not from home team", exception.Message);
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelHomeGoalInSecondHalfWithNoHomeGoals_ThrowsException()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "H;A" };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(MatchEvent.CancelHomeGoal));
        Assert.Equal("Cannot cancel home goal: last score is not from home team", exception.Message);
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelAwayGoalWithNoAwayGoals_ThrowsException()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "H" };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(MatchEvent.CancelAwayGoal));
        Assert.Equal("Cannot cancel away goal: last score is not from away team", exception.Message);
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelAwayGoalInSecondHalfWithNoAwayGoals_ThrowsException()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "A;H" };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(MatchEvent.CancelAwayGoal));
        Assert.Equal("Cannot cancel away goal: last score is not from away team", exception.Message);
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelAwayGoalInSecondHalf_RemovesLastAwayGoal()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HA;" };
        var model = new MatchEventModel(matchScore);

        // Act
        var result = model.HandleMatchEvent(MatchEvent.CancelAwayGoal);

        // Assert
        Assert.Equal("H;", result.Scores);
    }

    [Fact]
    public void HandleMatchEvent_WhenCancelHomeGoalInSecondHalfWithLastScoreAway_ThrowsException()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 91, Scores = "HA;" };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(MatchEvent.CancelHomeGoal));
        Assert.Equal("Cannot cancel home goal: last score is not from home team", exception.Message);
    }
} 