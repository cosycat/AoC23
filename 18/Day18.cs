using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace d18y2023; 

public static class Day18 {
    private const long ExpectedResultTest1 = 62;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay18.txt";
    private const string TestFileName = "testInputDay18.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 0; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 0; // For ensuring it stays correct, once the actual result is known
    
    private const string Success = "✅";
    private const string Fail = "❌";

    public static void Main(string[] args) {
        TestRun();

        Stopwatch sw = new();
        sw.Start();
        Solve(InputFileName, out var result1, out var result2);
        sw.Stop();
        PrintResult(result1, ActualResult1, 1);
        PrintResult(result2, ActualResult2, 2);
        Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
    }
    
    private static void PrintResult(long result, long expected, int index, bool isTest = false) {
        Console.WriteLine($"{(isTest ? "Test " : "")}Result {index}: {result} {(expected == 0 ? "" : expected == result ? Success : Fail + $"(expected: {expected})")} ");
    }
    
    [Conditional("DEBUG")]
    private static void TestRun() {
        Solve(TestFileName, out var resultTest1, out var resultTest2);
        PrintResult(resultTest1, ExpectedResultTest1, 1, true);
        if (Test2Started)
            PrintResult(resultTest2, ExpectedResultTest2, 2, true);
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");

        // Process input line by line with regex
        const string singleColorCode = @"[0-9a-f]{2}";
        const string colorCode = $@"#(?<r>{singleColorCode})(?<g>{singleColorCode})(?<b>{singleColorCode})";
        const string mainPattern = $@"(?<direction>[UDLR])\s(?<distance>\d+)\s\({colorCode}\)";
        // Regex for strings like "InputLine 1: 10,  2, 33,  4, 56, 78,  9"
        Console.WriteLine($"Regex: {mainPattern}");

        List<List<Tile>> map = new();

        map.Add(new List<Tile>());
        map[0].Add(new Tile(true));

        (int x, int y) currPos = (0, 0);

        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            if (line.Length == 0) {
                break;
            }

            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(),
                $"Line {i} does not match {mainMatch.Value}");

            var direction = mainMatch.Groups["direction"].Value switch {
                "U" => Direction.Up,
                "D" => Direction.Down,
                "L" => Direction.Left,
                "R" => Direction.Right,
                _ => throw new Exception($"Unknown direction {mainMatch.Groups["direction"].Value}")
            };
            // if (i == 0) {
            //     map[0][0].FilledFrom = direction;
            // }

            var distance = int.Parse(mainMatch.Groups["distance"].Value);

            var colorR = int.Parse(mainMatch.Groups["r"].Value, NumberStyles.HexNumber);
            var colorG = int.Parse(mainMatch.Groups["g"].Value, NumberStyles.HexNumber);
            var colorB = int.Parse(mainMatch.Groups["b"].Value, NumberStyles.HexNumber);
            var color = new Color(colorR, colorG, colorB);

            for (int j = 0; j < distance; j++) {
                currPos = DigInDirection(map, currPos, direction, color);
            }

            // Console.WriteLine($"Line {i}: {line} -> {mainMatch.Groups["direction"].Value} {mainMatch.Groups["distance"].Value} ({colorR}, {colorG}, {colorB})");
            // PrintMap(map);
        }

        PrintMap(map);

        // tried to hack fill it. didn't work.
        for (int y = 0; y < map.Count; y++) {
            var inside = false;
            for (int x = 0; x < map[0].Count; x++) {
                var tile = map[y][x];
                if (tile.IsDug) {
                    var prevDirection = tile.FilledFrom;
                    if (tile.FilledFrom is Direction.Up or Direction.Down) {
                        inside = !inside;
                        if (x + 1 >= map[y].Count || !map[y][x + 1].IsDug) continue;
                        if ((map[y][x + 1].FilledFrom == Direction.Up || map[y][x + 1].FilledFrom == Direction.Down) && map[y][x + 1].FilledFrom != prevDirection) continue; // just a normal wall
                        Console.WriteLine($"{y}: Started hor row at {x}, {y}! prev: {prevDirection}, curr: {map[y][x].FilledFrom}");
                        x++;
                    }

                    while (x < map[y].Count &&
                           map[y][x].IsDug && (map[y][x].FilledFrom == Direction.Right || 
                                               map[y][x].FilledFrom == Direction.Left)) {
                        x++;
                    }
                    
                    Debug.Assert(x < map[y].Count && map[y][x].IsDug && (map[y][x].FilledFrom == Direction.Up || map[y][x].FilledFrom == Direction.Down),
                        $"Tile at {x}, {y} at end of hor row should be up or down!");
                    
                    if (prevDirection == Direction.Up && map[y][x].FilledFrom == Direction.Down ||
                        prevDirection == Direction.Down && map[y][x].FilledFrom == Direction.Up) {
                        Console.WriteLine($"Row {y}: Tile at {x} is a 180 turn!");
                        inside = !inside; // it was just a 180 turn
                    }

                    if (prevDirection == map[y][x].FilledFrom) {
                        Console.WriteLine($"Row {y}: Tile at {x} is a straight line! prev: {prevDirection}, curr: {map[y][x].FilledFrom}");
                    }
                    if (prevDirection != map[y][x].FilledFrom) {
                        Console.WriteLine($"Row {y}: Tile at {x} is a turn! prev: {prevDirection}, curr: {map[y][x].FilledFrom}");
                    }
                    
                    

                    // if (tile.FilledFrom == Direction.Down && map[y][x + 1].FilledFrom == Direction.Up) {
                    //     inside = !inside; // it was just a 180 turn
                    // }
                    
                    // inside = !inside;
                    // while (x < map[y].Count && map[y][x].IsDug && (map[y][x].FilledFrom == Direction.Right || map[y][x].FilledFrom == Direction.Left)) {
                    //     x++;
                    // }
                }
                if (x < map[y].Count && inside) {
                    map[y][x].IsDug = true;
                }
            }
        }

        (int x, int y) startPos = (0, 1);
        while (!map[startPos.y][startPos.x].IsDug) {
            startPos.x++;
        }
        
        // FloodFill(map,  );
        
        PrintMap(map);
        
        result1 = map.Sum(row => row.Count(tile => tile.IsDug));
        
        
    }

    private static (int x, int y) DigInDirection(List<List<Tile>> map, (int x, int y) currPos, Direction direction, Color color) {
        (int x, int y) newPos;
        switch (direction) {
            case Direction.Up:
                if (map[currPos.y][currPos.x].FilledFrom == Direction.Left ||
                    map[currPos.y][currPos.x].FilledFrom == Direction.Right) {
                    map[currPos.y][currPos.x].FilledFrom = direction;
                }
                newPos = (currPos.x, currPos.y - 1);
                break;
            case Direction.Down:
                if (map[currPos.y][currPos.x].FilledFrom == Direction.Left ||
                    map[currPos.y][currPos.x].FilledFrom == Direction.Right) {
                    map[currPos.y][currPos.x].FilledFrom = direction;
                }
                newPos = (currPos.x, currPos.y + 1);
                break;
            case Direction.Left:
                // if (map[currPos.y][currPos.x].FilledFrom == Direction.Up ||
                //     map[currPos.y][currPos.x].FilledFrom == Direction.Down) {
                //     map[currPos.y][currPos.x].FilledFrom = direction;
                // }
                newPos = (currPos.x - 1, currPos.y);
                break;
            case Direction.Right:
                // if (map[currPos.y][currPos.x].FilledFrom == Direction.Up ||
                //     map[currPos.y][currPos.x].FilledFrom == Direction.Down) {
                //     map[currPos.y][currPos.x].FilledFrom = direction;
                // }
                newPos = (currPos.x + 1, currPos.y);
                break;
            default:
                throw new Exception($"Unknown direction {direction}");
        }

        if (newPos.y < 0) {
            map.Insert(0, new List<Tile>());
            for (int i = 0; i < map[1].Count; i++) {
                map[0].Add(new Tile(false));
            }
            newPos.y = 0;
        }
        if (newPos.x < 0) {
            for (int i = 0; i < map.Count; i++) {
                map[i].Insert(0, new Tile(false));
            }
            newPos.x = 0;
        }
        if (newPos.y >= map.Count) {
            map.Add(new List<Tile>());
            for (int i = 0; i < map[0].Count; i++) {
                map[^1].Add(new Tile(false));
            }
        }
        if (newPos.x >= map[0].Count) {
            for (int i = 0; i < map.Count; i++) {
                map[i].Add(new Tile(false));
            }
        }
        
        map[newPos.y][newPos.x].IsDug = true;
        if (map[newPos.y][newPos.x].FilledFrom == Direction.None ||
            map[newPos.y][newPos.x].FilledFrom == Direction.Up ||
            map[newPos.y][newPos.x].FilledFrom == Direction.Down) {
            map[newPos.y][newPos.x].FilledFrom = direction;
        }

        return newPos;
    }

    private static void PrintMap(List<List<Tile>> map) {
        for (int y = 0; y < map.Count; y++) {
            for (int x = 0; x < map[0].Count; x++) {
                // Debug.Assert(!map[y][x].IsDug || map[y][x].FilledFrom != Direction.None, $"Tile at {x}, {y} is dug but has no direction!");
                Console.Write(map[y][x].IsDug ? $"{GetCharForDirection(map[y][x].FilledFrom)}" : ".");
            }
            Console.WriteLine();
        }

        Console.WriteLine();
    }
    
    private static char GetCharForDirection(Direction direction) {
        return direction switch {
            Direction.Up => '^',
            Direction.Down => 'v',
            Direction.Left => '<',
            Direction.Right => '>',
            Direction.None => '#',
            _ => throw new Exception($"Unknown direction {direction}")
        };
    }
}

public class Tile {
    public Tile(bool isDug) {
        IsDug = isDug;
    }

    public bool IsDug { get; set; }
    public Direction FilledFrom { get; set; } = Direction.None;
}

public struct Color {
    public Color(int r, int g, int b) {
        R = r;
        G = g;
        B = b;
    }

    public int R { get; }
    public int G { get; }
    public int B { get; }
}

public enum Direction {
    Up,
    Down,
    Left,
    Right,
    None
}
