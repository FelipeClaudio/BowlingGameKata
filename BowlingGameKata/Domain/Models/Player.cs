namespace BowlingGameKata.Domain.Models;
public class Player(string name)
{
    public int Id { get; set; }
    public string Name { get; set; } = name;
}
