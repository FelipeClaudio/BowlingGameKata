using System.ComponentModel.DataAnnotations;

namespace BowlingGameKata.Domain.Models;

public class Game(int maxNumberOfPlayers = 4)
{
    public Frame Frame { get => GetFrameForPlayer(CurrentPlayer.Id); }

    private const int MaxGamePoints = 300;
    private const int NumberOfPins = 10;
    private const int MaxFrames = 10;

    private readonly List<Player> _players = [];
    private int _currentPlayerIndex = 0;

    private readonly List<Frame> _frames = [];
    private readonly int _maxNumberOfPlayers = maxNumberOfPlayers;

    private Player CurrentPlayer => _players[_currentPlayerIndex];

    public void Roll(int numberOfPinsKnockedDown)
    {
        if (_players.Count == 0)
            throw new InvalidOperationException("Cannot start game without players");

        if (numberOfPinsKnockedDown < 0 || numberOfPinsKnockedDown > NumberOfPins)
            throw new ArgumentOutOfRangeException(nameof(numberOfPinsKnockedDown));

        var incrementedCurrentPlayerScore = GetFrameForPlayer(_currentPlayerIndex).Score + numberOfPinsKnockedDown;
        if (incrementedCurrentPlayerScore > MaxGamePoints)
            throw new ValidationException($"Cannot add {numberOfPinsKnockedDown} to " +
                $"player {CurrentPlayer.Name} as it would exceed the maximum gaming pontuation of {MaxGamePoints}");

        AddScore(numberOfPinsKnockedDown);

        var currentFrameForPlayer = GetFrameForPlayer(_currentPlayerIndex);

        if (currentFrameForPlayer.CurrentTry == 0 && incrementedCurrentPlayerScore == NumberOfPins)
            currentFrameForPlayer.ScoredAStrike = true;

        if (currentFrameForPlayer.CurrentTry == 1 && incrementedCurrentPlayerScore == NumberOfPins)
            currentFrameForPlayer.ScoredASpare = true;

        if (currentFrameForPlayer.Id < (MaxFrames - 1) && (currentFrameForPlayer.CurrentTry == 1 || incrementedCurrentPlayerScore == NumberOfPins))
        {
            MoveToNextPlay(currentFrameForPlayer);
            return;
        }
        currentFrameForPlayer.CurrentTry++;

        if (currentFrameForPlayer.Id == (MaxFrames - 1) && currentFrameForPlayer.CurrentTry == 3)
            return;

        if (currentFrameForPlayer.Id < (MaxFrames - 1) && currentFrameForPlayer.CurrentTry == 2)
        {
            MoveToNextPlay(currentFrameForPlayer);
        }
    }

    private void MoveToNextPlay(Frame currentFrameForPlayer)
    {
        currentFrameForPlayer.IsLatest = false;
        _frames[currentFrameForPlayer.Id + 1].IsLatest = true;

        if (_players.Count > 1) 
        {
            var newPlayerIndex = (currentFrameForPlayer.PlayerId + 1) % _players.Count;
            _currentPlayerIndex = newPlayerIndex;
        }
    }

    public int Score() =>
        _frames
            .Where(frame => frame.PlayerId == _currentPlayerIndex)
            .Sum(frame => frame.Score);

    public int GetPlayerScore(int playerId) => 
        _frames
            .Where(frame => frame.PlayerId == playerId)
            .Sum(frame => frame.Score);

    public void AddScore(int score)
    {
        var currentFrameForPlayer = GetFrameForPlayer(_currentPlayerIndex);
        currentFrameForPlayer.Score += score;
        currentFrameForPlayer.Rolls.Add(new Roll { Id = CurrentPlayer.CurrentRoll });

        AddScoreToPreviousRoll(score);
        AddScoreForTheFrameBeforeTheRoll(score);

        CurrentPlayer.CurrentRoll++;
    }

    private void AddScoreToPreviousRoll(int score)
    {
        var previousFrameForPlayer = _frames
            .SingleOrDefault(frame => frame.Rolls.Any(roll => roll.Id == CurrentPlayer.CurrentRoll - 1) && frame.PlayerId == CurrentPlayer.Id);
        var currentFrameForPlayer = GetFrameForPlayer(_currentPlayerIndex);

        if (previousFrameForPlayer == null)
            return;

        if (currentFrameForPlayer.Id == (MaxFrames - 1) && currentFrameForPlayer.CurrentTry > 1)
            return;

        if ((CurrentPlayer.CurrentRoll - previousFrameForPlayer.Rolls.Max(roll => roll.Id)) > 1)
            return;

        if (GetFrameForPlayer(_currentPlayerIndex).Id == previousFrameForPlayer.Id)
            return;

        if (previousFrameForPlayer.ScoredASpare || previousFrameForPlayer.ScoredAStrike)
        {
            previousFrameForPlayer.Score += score;
        }
    }

    private void AddScoreForTheFrameBeforeTheRoll(int score)
    {
        var rollBeforePreviousForPlayer = _frames
            .SingleOrDefault(frame => frame.Rolls.Any(roll => roll.Id == CurrentPlayer.CurrentRoll - 2));

        if (rollBeforePreviousForPlayer == null)
            return;

        if (GetFrameForPlayer(_currentPlayerIndex).Id == rollBeforePreviousForPlayer.Id)
            return;

        if (rollBeforePreviousForPlayer.ScoredAStrike)
        {
            rollBeforePreviousForPlayer.Score += score;
        }
    }

    private Frame GetFrameForPlayer(int playerId) =>
        _frames.SingleOrDefault(frame => frame.PlayerId == playerId && frame.IsLatest) ?? new Frame();

    public void AddPlayer(Player player)
    {
        if (_players.Count == _maxNumberOfPlayers)
            throw new InvalidOperationException($"Cannot have more than {_maxNumberOfPlayers} in this bowling game");

        player.Id = _players.Count;

        for (int i = 0; i < NumberOfPins; i++)
        {
            var frame = new Frame
            {
                Id = i,
                PlayerId = player.Id
            };

            if (i == 0)
                frame.IsLatest = true;

            _frames.Add(frame);
        }

        _players.Add(player);
    }
}
