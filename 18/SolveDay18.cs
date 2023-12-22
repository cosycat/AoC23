using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace d18y2023;

public static partial class Day18 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay18_00.txt", 62, 952408144115)
    };

    private const /*resultType*/ int ActualResult1 = 56678;
    private const /*resultType*/ int ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        
        const string mainPattern = @"(?<direction>[UDLR])\s(?<distance>\d+)\s\(#(?<distance2>[0-9a-f]{5})(?<direction2>[0-9a-f])\)";
        Console.WriteLine($"Regex: {mainPattern}");

        var grid1 = new Grid();
        var grid2 = new Grid();
        
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            if (line.Length == 0) {
                break;
            }

            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(),
                $"Line {i} does not match {mainMatch.Value}");

            // PART 1
            var direction = mainMatch.Groups["direction"].Value switch {
                "U" => Direction.Up,
                "D" => Direction.Down,
                "L" => Direction.Left,
                "R" => Direction.Right,
                _ => throw new Exception($"Unknown direction {mainMatch.Groups["direction"].Value}")
            };
            var distance = int.Parse(mainMatch.Groups["distance"].Value);
            grid1.AddCoordinate(direction, distance);
            
            // PART 2
            var direction2 = mainMatch.Groups["direction2"].Value switch {
                "0" => Direction.Right,
                "1" => Direction.Down,
                "2" => Direction.Left,
                "3" => Direction.Up,
                _ => throw new Exception($"Unknown direction {mainMatch.Groups["direction2"].Value}")
            };
            var distance2 = int.Parse(mainMatch.Groups["distance2"].Value, NumberStyles.HexNumber);
            grid2.AddCoordinate(direction2, distance2);
        }
    }
}

public class Grid {
    private readonly List<(long x, long y)> Coordinates = new() { (0, 0) };
    private long Circumference { get; set; } = 0;
    public long Area { get; private set; }

    public void AddCoordinate(Direction direction, long distance) {
        Circumference += distance;
        var newCoord = (Coordinates[^1].x, Coordinates[^1].y);
        switch (direction) {
            case Direction.Up:
                newCoord.y += distance;
                break;
            case Direction.Down:
                newCoord.y -= distance;
                break;
            case Direction.Left:
                newCoord.x -= distance;
                break;
            case Direction.Right:
                newCoord.x += distance;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
        if (Coordinates.Count <= 2) return;
        var prevCoord = Coordinates[^1];

        var newArea = CalcArea(Coordinates[0], prevCoord, newCoord);
        Area += newArea;
        Coordinates.Add(newCoord);
    }

    private long CalcArea((long x, long y) a, (long x, long y) b, (long x, long y) c) {
        // ground: b-c
        // height: a
        Debug.Assert(b.x == c.x || b.y == c.y);
        var height = (b.x == c.x) ? b.x - a.x : b.y - a.y;
        if (b.x == c.x) {
            // vert
            var yDiff = b.y - c.y;
        }
        else {
            // hor
            var xDiff = b.x - c.x;
        }

        throw new NotImplementedException();
    }
}

public enum Direction {
    Up,
    Down,
    Left,
    Right,
}