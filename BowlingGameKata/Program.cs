// See https://aka.ms/new-console-template for more information
using BowlingGameKata.Domain.Models;

var game = new Game();
game.Roll(5);
var score = game.Score();

Console.WriteLine($"My score is {score}");