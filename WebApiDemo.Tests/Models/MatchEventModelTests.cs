using WebApiDemo.Exceptions;
using WebApiDemo.Models;
using Xunit;

namespace WebApiDemo.Tests.Models;

public class MatchEventModelTests
{
    [Theory]
    [InlineData("", MatchEvent.HomeGoal, "1-0 (First Half)")]
    [InlineData("", MatchEvent.AwayGoal, "0-1 (First Half)")]
    [InlineData("H", MatchEvent.HomeGoal, "2-0 (First Half)")]
    [InlineData("A", MatchEvent.AwayGoal, "0-2 (First Half)")]
    [InlineData("H;", MatchEvent.HomeGoal, "2-0 (Second Half)")]
    [InlineData("A;", MatchEvent.AwayGoal, "0-2 (Second Half)")]
    [InlineData("H;H", MatchEvent.HomeGoal, "3-0 (Second Half)")]
    [InlineData("A;A", MatchEvent.AwayGoal, "0-3 (Second Half)")]
    [InlineData("H", MatchEvent.CancelHomeGoal, "0-0 (First Half)")]
    [InlineData("A", MatchEvent.CancelAwayGoal, "0-0 (First Half)")]
    [InlineData("H;", MatchEvent.CancelHomeGoal, "0-0 (Second Half)")]
    [InlineData("A;", MatchEvent.CancelAwayGoal, "0-0 (Second Half)")]
    [InlineData("H;H", MatchEvent.CancelHomeGoal, "1-0 (Second Half)")]
    [InlineData("A;A", MatchEvent.CancelAwayGoal, "0-1 (Second Half)")]
    [InlineData("", MatchEvent.NextPeriod, "0-0 (Second Half)")]
    [InlineData("H", MatchEvent.NextPeriod, "1-0 (Second Half)")]
    [InlineData("A", MatchEvent.NextPeriod, "0-1 (Second Half)")]
    public void HandleMatchEvent_WithDifferentEvents_ReturnsCorrectScore(string initialScores, MatchEvent matchEvent, string expectedScore)
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 1, Scores = initialScores };
        var model = new MatchEventModel(matchScore);

        // Act
        var updatedModel = new MatchEventModel(model.HandleMatchEvent(matchEvent));

        // Assert
        Assert.Equal(expectedScore, updatedModel.GetFormattedScore());
    }

    [Theory]
    [InlineData("", MatchEvent.CancelHomeGoal, "Cannot cancel home goal: last score is not from home team")]
    [InlineData("", MatchEvent.CancelAwayGoal, "Cannot cancel away goal: last score is not from away team")]
    [InlineData("A", MatchEvent.CancelHomeGoal, "Cannot cancel home goal: last score is not from home team")]
    [InlineData("H", MatchEvent.CancelAwayGoal, "Cannot cancel away goal: last score is not from away team")]
    [InlineData("H;A", MatchEvent.CancelHomeGoal, "Cannot cancel home goal: last score is not from home team")]
    [InlineData("A;H", MatchEvent.CancelAwayGoal, "Cannot cancel away goal: last score is not from away team")]
    public void HandleMatchEvent_WhenCancelGoalWithWrongTeam_ThrowsException(string initialScores, MatchEvent matchEvent, string expectedMessage)
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 1, Scores = initialScores };
        var model = new MatchEventModel(matchScore);

        // Act & Assert
        var exception = Assert.Throws<InvalidMatchEventException>(() => model.HandleMatchEvent(matchEvent));
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 1, Scores = "" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(1, model.MatchId);
        Assert.Equal("0-0 (First Half)", model.GetFormattedScore());
    }

    [Fact]
    public void Constructor_WithFirstHalfScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 1, Scores = "HH" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(1, model.MatchId);
        Assert.Equal("2-0 (First Half)", model.GetFormattedScore());
    }

    [Fact]
    public void Constructor_WithSecondHalfScore_InitializesCorrectly()
    {
        // Arrange
        var matchScore = new MatchScore { MatchId = 1, Scores = "H;A" };

        // Act
        var model = new MatchEventModel(matchScore);

        // Assert
        Assert.Equal(1, model.MatchId);
        Assert.Equal("1-1 (Second Half)", model.GetFormattedScore());
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