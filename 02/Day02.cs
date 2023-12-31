using System.Diagnostics;
using System.Text.RegularExpressions;

namespace _02; 

public static class Day02 {
    
    public static void Main(string[] args) {
        var allLines = File.ReadAllLines("input.txt").ToList();

        const string rgbPattern = @"(?'rgb'(red|green|blue))";
        const string colorPattern = $@"(?'Color'\s\d+\s{rgbPattern},?)";
        const string gamePattern = $@"Game \d+:(?'Draws'{colorPattern}+;?)+";
        Console.WriteLine($"Regex: {gamePattern}");
        
        var solution1 = 0; // Game numbers of possible games added together
        var solution2 = 0; // Power of min colors of all games multiplied together

        for (var i = 0; i < allLines.Count; i++) {
            // GAME
            var game = allLines[i];
            var gameNumber = i + 1;
            var isGamePossible = true;
            var minColors = new []{0, 0, 0};
            
            // var pattern = @"Game \d+:(?'Draws'(?'Color'\s\d+\s\w+,?)+;?)+";
            var match = Regex.Match(game, gamePattern);
            
            Debug.Assert(match.Success, $"game {gameNumber} does not match {gamePattern}");
            Debug.Assert(game == match.Value, $"game {gameNumber} does not match {match.Value}");
            
            Console.WriteLine($"{gameNumber}: {match.Value}");
            foreach (Capture drawCapture in match.Groups["Draws"].Captures) {
                // DRAW
                Console.WriteLine($"  {drawCapture.Value}");
                var colorMatch = Regex.Match(drawCapture.Value, $@"({colorPattern})+;?");
                Debug.Assert(colorMatch.Success && colorMatch.Value == drawCapture.Value, $"draw {drawCapture.Value} does not match {colorMatch.Value}");
                
                foreach (Capture colorCapture in colorMatch.Groups["Color"].Captures) {
                    // COLOR
                    Console.WriteLine($"    {colorCapture.Value}");
                    var numberPatternMatch = Regex.Match(colorCapture.Value, $@"\s(?<Number>\d+)\s(?<Color>{rgbPattern}),?");
                    Debug.Assert(numberPatternMatch.Success && numberPatternMatch.Value == colorCapture.Value, $"color {colorCapture.Value} does not match {numberPatternMatch.Value}");
                    if (!numberPatternMatch.Groups.TryGetValue("Number", out var numberGroup)) Debug.Fail("number group not found");
                    if (!numberPatternMatch.Groups.TryGetValue("Color", out var colorGroup)) Debug.Fail("color group not found");
                    var number = int.Parse(numberGroup.Value);
                    if (IsTooManyColors(colorGroup.Value, number)) {
                        Console.WriteLine($"      {colorGroup.Value} {number} is too many");
                        isGamePossible = false;
                    }
                    var colorIndex = colorGroup.Value switch {
                        "red" => 0,
                        "green" => 1,
                        "blue" => 2,
                        _ => throw new ArgumentOutOfRangeException(nameof(colorGroup), colorGroup.Value, null)
                    };
                    minColors[colorIndex] = Math.Max(minColors[colorIndex], number);
                }
            }
            
            Console.WriteLine($"game {gameNumber} is {(isGamePossible ? "possible" : "impossible")}");
            if (isGamePossible) solution1 += gameNumber;
            
            Console.WriteLine($"min colors: {string.Join(", ", minColors)}");
            solution2 += minColors.Aggregate(1, (acc, x) => acc * x);
        }
        
        Console.WriteLine($"solution1: {solution1}");
        Console.WriteLine($"solution2: {solution2}");
    }
    
    public static bool IsTooManyColors(string color, int number) => color switch {
        "red" => number > 12,
        "green" => number > 13,
        "blue" => number > 14,
        _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
    };
    
}