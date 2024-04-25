using BowlingGameKata.Domain.Models;
using FluentAssertions;

namespace BowlingGameKataTests;

public class GameTests
{
    private const int NumberOfPins = 10;
    private const int MaxNumberOfFrames = 10;

    [InlineData(1)]
    [InlineData(5)]
    [InlineData(6)]
    [Theory(DisplayName ="Game | When player scored less than 10 pins in a roll | Should return regular score.")]
    public void Game_WhenPlayerScoredLessThan10Pin_ShouldReturnRegularScore(int score)
    {
        // Arrange
        var game = new Game();
        game.AddPlayer(new Player("Player 1"));

        // Act
        game.Roll(score);
        var result = game.Score();

        // Assert
        result.Should().Be(score);
    }

    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [Theory(DisplayName ="Game | When two rolls were done | Should move to new frame.")]
    public void Game_WhenTwoRollsWereDone_ShouldMoveToNewFrame(int numberOfFrames)
    {
        // Arrange
        var game = new Game();
        game.AddPlayer(new Player("Player 1"));

        // Act
        for (int i = 0; i < numberOfFrames - 1; i++)
        {
            game.Roll(4);
            game.Roll(3);
        }

        // Assert
        game.Frame.Id.Should().Be(numberOfFrames - 1);
    }

    [Fact(DisplayName ="Game | When game is in the last frame | Should be possible to play 3 times to strike all pins.")]
    public void Game_WhenGameIsInTheLastFrame_ShouldBePossibleToPlay3TimesToStrikeAllPins()
    {
        // Arrange
        var game = new Game();
        game.AddPlayer(new Player("Player 1"));

        // Act
        for (int i = 0; i < MaxNumberOfFrames; i++)
        {
            game.Roll(2);
            game.Roll(7);
        }

        // Assert
        game.Frame.Id.Should().Be(MaxNumberOfFrames - 1);
    }

    [Fact(DisplayName = "Game | When player strikes in the first roll | Should move to new frame.")]
    public void Game_WhenPlayerStrikesInTheFirstRoll_ShouldMoveToNewFrame()
    {
        // Arrange
        var game = new Game();
        game.AddPlayer(new Player("Player 1"));

        // Act
        game.Roll(NumberOfPins);

        // Assert
        game.Frame.Id.Should().Be(1);
    }

    [Fact(DisplayName = "Game | When player strikes in the first roll | Should add next 2 rolls points.")]
    public void Game_WhenPlayerScoredStrikeOnFirstRoll_ShouldAddNextRollsPointToTheFirstScore()
    {
        // Arrange
        var game = new Game();
        var player = new Player("Player 1");
        game.AddPlayer(player);

        // Act
        // First Frame
        game.Roll(NumberOfPins);

        // Second Frame
        game.Roll(2);
        game.Roll(5);

        // Third Frame
        game.Roll(6);
        game.Roll(2);

        // Assert
        // First frame score = 10 + 2 + 5 = 17
        // Second frame score = 2 + 5 = 7
        // Third frame score = 6 + 2 = 8
        // Total = 40
        game.Score().Should().Be(32);
    }

    [Fact(DisplayName = "Game | When player plays a perfect game | Should have a final score of 300.")]
    public void Game_WhenPlayerPlaysAPerfectGame_ShouldHaveAFinalScoreOf300()
    {
        // Arrange
        var game = new Game();
        var player = new Player("Player 1");
        game.AddPlayer(player);
        const int NumberOfConsecutiveStrikesInAPerfectGame = 12;

        // Act
        for (int i = 0; i < NumberOfConsecutiveStrikesInAPerfectGame; i++)
        {
            game.Roll(NumberOfPins);
        }

        // Assert
        game.Score().Should().Be(300);
    }

    [Fact(DisplayName = "Game | When player does the best possible game without strikes | Should have a final score of 190.")]
    public void Game_WhenPlayerPlaysDoesTheBestPossibleGameWithoutStrikes_ShouldHaveAFinalScoreOf190()
    {
        // Arrange
        var game = new Game();
        var player = new Player("Player 1");
        game.AddPlayer(player);

        // Act
        for(int i = 0; i < MaxNumberOfFrames; i++)
        {
            game.Roll(9);
            game.Roll(1);
        }

        // A player can Roll one extra time in the last frame.
        game.Roll(9);

        // Assert
        game.Score().Should().Be(190);
    }

    [Fact(DisplayName = "Game | When two players are added | Should track points for both players.")]
    public void Game_TwoPlayersAreAdded_ShouldTrackPointsForBothPlayers()
    {
        // Arrange
        var game = new Game();
        var player1= new Player("Player 1");
        game.AddPlayer(player1);
        var player2 = new Player("Player 2");
        game.AddPlayer(player2);

        // Act
        // Player 1
        game.Roll(3);
        game.Roll(5);

        // Player 2
        game.Roll(2);
        game.Roll(4);

        // Assert
        game.GetPlayerScore(player1.Id).Should().Be(8);
        game.GetPlayerScore(player2.Id).Should().Be(6);
    }

    [Fact(DisplayName = "Game | When adding more players than allowed by the game | Should throw exception.")]
    public void Game_AddingMorePlayersThanAllowedByTheGame_ShouldThrowException()
    {
        // Arrange
        const int maxNumberOfPlayers = 4;
        var game = new Game(maxNumberOfPlayers);
        for (int i = 0; i < maxNumberOfPlayers; i++)
        {
            game.AddPlayer(new Player($"Player {i + 1}"));
        }

        // Act
        Action act = () => game.AddPlayer(new Player($"Player {maxNumberOfPlayers + 1}")); 

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [InlineData(-5)]
    [InlineData(11)]
    [Theory(DisplayName ="Game | When invalid number of knocked down pins is provided | Should return exception.")]
    public void Game_WhenInvalidNubmerOfKnockedDownPinsIsProvided_ShouldReturnException(int score)
    {
        // Arrange
        var game = new Game();
        game.AddPlayer(new Player("Player 1"));

        // Act
        Action act = () => game.Roll(score);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact(DisplayName = "Game | When no player is added | Should return exception.")]
    public void Game_WhenNoPlayerIsAdded_ShouldReturnException()
    {
        // Arrange
        var game = new Game();

        // Act
        Action act = () => game.Roll(1);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}