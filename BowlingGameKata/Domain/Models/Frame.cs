namespace BowlingGameKata.Domain.Models;

public class Frame
{
    public int PlayerId { get; set; }
    public int Id { get; set; }
    public int Score { get; set; }
    public bool ScoredAStrike { get; set; }
    public bool ScoredASpare { get; set; }
    public List<Roll> Rolls { get; set; } = [];
}
