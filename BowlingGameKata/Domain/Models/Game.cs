namespace BowlingGameKata.Domain.Models;

public class Game(int maxNumberOfPlayers = 4)
{
    public Frame Frame { get => GetFrameForPlayer(CurrentPlayer.Id); }

    public Player? Winner = null;

    private const int NumberOfPins = 10;
    private const int MaxFrames = 10;

    private readonly List<Player> _players = [];
    private int _currentPlayerIndex = 0;
    private bool IsTheFirstRollInsideFrame => Frame.CurrentTry == 0;
    private bool IsTheSecondRollInsideFrame => Frame.CurrentTry == 1;
    private bool IsTheThirdRollInsideFrame => Frame.CurrentTry == 2;
    private bool IsTheLastFrameOfTheGame => Frame.Id == (MaxFrames - 1);
    private readonly List<Frame> _frames = [];
    private readonly int _maxNumberOfPlayers = maxNumberOfPlayers;

    private Player CurrentPlayer => _players[_currentPlayerIndex];

    public void Roll(int numberOfPinsKnockedDown)
    {
        if (_players.Count == 0)
            throw new InvalidOperationException("Cannot start game without players");

        if (numberOfPinsKnockedDown < 0 || numberOfPinsKnockedDown > NumberOfPins)
            throw new ArgumentOutOfRangeException(nameof(numberOfPinsKnockedDown));

        if (Winner is not null)
            throw new InvalidOperationException("Cannot play anymore as the game is already finished.");

        var currentFrameForPlayer = GetFrameForPlayer(_currentPlayerIndex);

        AddScore(currentFrameForPlayer, numberOfPinsKnockedDown);

        if (!IsTheLastFrameOfTheGame && (IsTheSecondRollInsideFrame || currentFrameForPlayer.ScoredAStrike))
        {
            MoveToNextPlay(currentFrameForPlayer);
            return;
        }

        bool isTheLastRollForPlayer = IsTheLastFrameOfTheGame && IsTheThirdRollInsideFrame;
        currentFrameForPlayer.CurrentTry++;
        if (!isTheLastRollForPlayer)
        {
            return;
        }

        bool isTheLastPlayerToPlayTheGame = _currentPlayerIndex == _players.Count - 1;
        if (isTheLastPlayerToPlayTheGame)
        {
            Winner = SetGameWinner(); 
            return;
        }

        MoveToNextPlay(currentFrameForPlayer);
    }

    private Player SetGameWinner()
    {
        var winnerPlayerId = _frames.GroupBy(frame => frame.PlayerId)
            .ToDictionary(group => group.Key, group => group.Sum(frame => frame.Score))
            .OrderByDescending(x => x.Value)
            .First().Key;

        return _players[winnerPlayerId];
    }

    private void MoveToNextPlay(Frame currentFrameForPlayer)
    {
        currentFrameForPlayer.IsLatest = false;
        var nextFrame = _frames
            .SingleOrDefault(frame => frame.Id == currentFrameForPlayer.Id + 1 
                && frame.PlayerId == _currentPlayerIndex);

        if (nextFrame is not null)
            nextFrame.IsLatest = true;

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

    public void AddScore(Frame currentFrameForPlayer, int numberOfPinsKnockedDown)
    {
        var incrementedCurrentPlayerScore = currentFrameForPlayer.Score + numberOfPinsKnockedDown;

        currentFrameForPlayer.Score += numberOfPinsKnockedDown;
        currentFrameForPlayer.Rolls.Add(new Roll { Id = CurrentPlayer.CurrentRoll });

        currentFrameForPlayer.ScoredAStrike = IsTheFirstRollInsideFrame && incrementedCurrentPlayerScore == NumberOfPins;
        currentFrameForPlayer.ScoredASpare = IsTheSecondRollInsideFrame && incrementedCurrentPlayerScore == NumberOfPins;

        AddScoreToPreviousRoll(currentFrameForPlayer, numberOfPinsKnockedDown);
        AddScoreForTheFrameBeforeTheRoll(currentFrameForPlayer, numberOfPinsKnockedDown);

        CurrentPlayer.CurrentRoll++;
    }

    private void AddScoreToPreviousRoll(Frame currentFrameForPlayer, int score)
    {
        var previousFrameForPlayer = _frames
            .SingleOrDefault(frame => frame.Rolls.Any(roll => roll.Id == CurrentPlayer.CurrentRoll - 1) 
                && frame.PlayerId == CurrentPlayer.Id);

        if (previousFrameForPlayer is null)
            return;

        bool isTheLastRollOfTheLastFrame = IsTheLastFrameOfTheGame && IsTheSecondRollInsideFrame;
        bool hasMoreThan1RollBetweenCurrentAndLastFrame = (CurrentPlayer.CurrentRoll - previousFrameForPlayer.Rolls.Max(roll => roll.Id)) > 1;
        bool isPreviousRollInTheSameFrame = currentFrameForPlayer.Id == previousFrameForPlayer.Id;

        if (isTheLastRollOfTheLastFrame || hasMoreThan1RollBetweenCurrentAndLastFrame || isPreviousRollInTheSameFrame)
            return;

        if (previousFrameForPlayer.ScoredASpare || previousFrameForPlayer.ScoredAStrike)
            previousFrameForPlayer.Score += score;
    }

    private void AddScoreForTheFrameBeforeTheRoll(Frame currentFrameForPlayer, int score)
    {
        var rollBeforePreviousForPlayer = _frames
            .SingleOrDefault(frame => frame.Rolls.Any(roll => roll.Id == CurrentPlayer.CurrentRoll - 2) 
            && frame.PlayerId == CurrentPlayer.Id);

        if (rollBeforePreviousForPlayer is null)
            return;

        bool rollBeforePreviousIsInTheSameFrame = currentFrameForPlayer.Id == rollBeforePreviousForPlayer.Id;
        if (rollBeforePreviousIsInTheSameFrame)
            return;

        if (rollBeforePreviousForPlayer.ScoredAStrike)
            rollBeforePreviousForPlayer.Score += score;
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