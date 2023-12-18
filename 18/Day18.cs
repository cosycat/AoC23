using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace d18y2023; 

public static class Day18 {
    private const long ExpectedResultTest1 = 62;
    private const long ExpectedResultTest2 = 952408144115;
    private const string InputFileName = "inputDay18.txt";
    private const string TestFileName = "testInputDay18.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 56678; // For ensuring it stays correct, once the actual result is known
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
        const string colorCode = @"";
        const string mainPattern = @"(?<direction>[UDLR])\s(?<distance>\d+)\s\(#(?<distance2>[0-9a-f]{5})(?<direction2>[0-9a-f])\)";
        // Regex for strings like "InputLine 1: 10,  2, 33,  4, 56, 78,  9"
        Console.WriteLine($"Regex: {mainPattern}");

        List<List<Tile>> map1 = new();
        List<List<Tile>> map2 = new();

        map1.Add(new List<Tile>());
        map1[0].Add(new Tile(true));
        map2.Add(new List<Tile>());
        map2[0].Add(new Tile(true));

        (int x, int y) currPos1 = (0, 0);
        
        List<(long skipStartX, long skipEndX)> skipped = new();
        Dictionary<int, long> xSolver = new();
        Dictionary<int, long> ySolver = new();
        
        var instructions2 = new List<(Direction direction, int distance)>();

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
            for (int j = 0; j < distance; j++) {
                currPos1 = DigInDirection(map1, currPos1, direction);
            }
            
            // PART 2
            var direction2 = mainMatch.Groups["direction2"].Value switch {
                "0" => Direction.Right,
                "1" => Direction.Down,
                "2" => Direction.Left,
                "3" => Direction.Up,
                _ => throw new Exception($"Unknown direction {mainMatch.Groups["direction2"].Value}")
            };
            var distance2 = int.Parse(mainMatch.Groups["distance2"].Value, NumberStyles.HexNumber);
            instructions2.Add((direction2, distance2));
        }

        // PrintMap(map1);

        result1 = CountInside(map1);
        
        
        // PART 2
        List<(int startX, int endX)> xRanges = new();
        (long x, long y) currPos2 = (0, 0);

        for (int i = 0; i < instructions2.Count; i++) {
            var instruction = instructions2[i];
        }
        
    }

    private static long CountInside(List<List<Tile>> map1) {
        long result1;
        
        // fill inside
        for (int y = 0; y < map1.Count; y++) {
            var inside = false;
            for (int x = 0; x < map1[0].Count; x++) {
                var tile = map1[y][x];
                if (tile.IsDug) {
                    var prevDirection = tile.FilledFrom;
                    if (tile.FilledFrom is Direction.Up or Direction.Down) {
                        inside = !inside;
                        if (x + 1 >= map1[y].Count || !map1[y][x + 1].IsDug) continue;
                        if ((map1[y][x + 1].FilledFrom == Direction.Up || map1[y][x + 1].FilledFrom == Direction.Down) && map1[y][x + 1].FilledFrom != prevDirection) continue; // just a normal wall
                        // Console.WriteLine($"{y}: Started hor row at {x}, {y}! prev: {prevDirection}, curr: {map1[y][x].FilledFrom}");
                        x++;
                    }

                    while (x < map1[y].Count &&
                           map1[y][x].IsDug && (map1[y][x].FilledFrom == Direction.Right || 
                                                map1[y][x].FilledFrom == Direction.Left)) {
                        x++;
                    }
                    
                    Debug.Assert(x < map1[y].Count && map1[y][x].IsDug && (map1[y][x].FilledFrom == Direction.Up || map1[y][x].FilledFrom == Direction.Down),
                        $"Tile at {x}, {y} at end of hor row should be up or down!");
                    
                    if (prevDirection == Direction.Up && map1[y][x].FilledFrom == Direction.Down ||
                        prevDirection == Direction.Down && map1[y][x].FilledFrom == Direction.Up) {
                        // Console.WriteLine($"Row {y}: Tile at {x} is a 180 turn!");
                        inside = !inside; // it was just a 180 turn
                    }

                    if (prevDirection == map1[y][x].FilledFrom) {
                        // Console.WriteLine($"Row {y}: Tile at {x} is a straight line! prev: {prevDirection}, curr: {map1[y][x].FilledFrom}");
                    }
                    if (prevDirection != map1[y][x].FilledFrom) {
                        // Console.WriteLine($"Row {y}: Tile at {x} is a turn! prev: {prevDirection}, curr: {map1[y][x].FilledFrom}");
                    }
                    
                    

                    // if (tile.FilledFrom == Direction.Down && map[y][x + 1].FilledFrom == Direction.Up) {
                    //     inside = !inside; // it was just a 180 turn
                    // }
                    
                    // inside = !inside;
                    // while (x < map[y].Count && map[y][x].IsDug && (map[y][x].FilledFrom == Direction.Right || map[y][x].FilledFrom == Direction.Left)) {
                    //     x++;
                    // }
                }
                if (x < map1[y].Count && inside) {
                    map1[y][x].IsDug = true;
                }
            }
        }
        
        PrintMap(map1);
        
        result1 = map1.Sum(row => row.Count(tile => tile.IsDug));
        return result1;
    }

    private static (int x, int y) DigInDirection(List<List<Tile>> map, (int x, int y) currPos, Direction direction) {
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

    public long X { get; set; } = 0;
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
