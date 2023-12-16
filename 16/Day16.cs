using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d16y2023; 

public static class Day16 {
    private const long ExpectedResultTest1 = 46;
    private const long ExpectedResultTest2 = 51;
    private const string InputFileName = "inputDay16.txt";
    private const string TestFileName = "testInputDay16.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 7236; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 7521; // For ensuring it stays correct, once the actual result is known
    
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
        var width = allLines[0].Length;
        var height = allLines.Count; // .Length;
        
        var grid = new Tile[width + 2, height + 2];
        
        // Process input char by char
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                grid[x + 1, y + 1] = new Tile(c);
            }
        }

        for (int y = 0; y < height + 2; y++) {
            grid[0, y] = Tile.EndTile;
            grid[width + 1, y] = Tile.EndTile;
        }
        for (int x = 0; x < width + 2; x++) {
            grid[x, 0] = Tile.EndTile;
            grid[x, height + 1] = Tile.EndTile;
        }

        // PrintGrid(grid);
        
        result1 = CountEnergized(1, 1, grid, width, height, Direction.Right);
        
        for (int y = 1; y < height + 1; y++) {
            var newEnergizedA = CountEnergized(1, y, grid, width, height, Direction.Right);
            var newEnergizedB = CountEnergized(width, y, grid, width, height, Direction.Left);
            result2 = Math.Max(result2, Math.Max(newEnergizedA, newEnergizedB));
        }
        for (int x = 0; x < width + 2; x++) {
            var newEnergizedA = CountEnergized(x, 1, grid, width, height, Direction.Down);
            var newEnergizedB = CountEnergized(x, height, grid, width, height, Direction.Up);
            result2 = Math.Max(result2, Math.Max(newEnergizedA, newEnergizedB));
        }
    }

    private static int CountEnergized(int startX, int startY, Tile[,] grid, int width, int height, Direction startDir) {
        var stack = new Stack<(int x, int y, Direction direction)>();
        stack.Push((startX, startY, startDir));
        while (stack.Count > 0) {
            var (x, y, direction) = stack.Pop();
            // Console.WriteLine($"Next Step: ({x}, {y}) {direction}");
            // PrintGrid(grid);
            var t = grid[x, y];
            if (t.IsEnergizedInDirection[direction]) {
                continue;
            }
            t.IsEnergizedInDirection[direction] = true;
            var (newDir, split) = t.GetNewDir(direction);
            Debug.Assert(!split || newDir == Direction.Right || newDir == Direction.Down, "When split, always give the same one direction");
            var (newX, newY) = newDir switch {
                Direction.Right => (x + 1, y),
                Direction.Left => (x - 1, y),
                Direction.Up => (x, y - 1),
                Direction.Down => (x, y + 1),
                _ => throw new ArgumentOutOfRangeException()
            };
            stack.Push((newX, newY, newDir));
            if (split) {
                stack.Push((newX - (2 * (newX - x)), newY - (2 * (newY - y)), newDir == Direction.Right ? Direction.Left : Direction.Up));
            }
        }

        var energized = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                energized += grid[x + 1, y + 1].IsEnergized ? 1 : 0;
                // reset grid
                grid[x + 1, y + 1].IsEnergizedInDirection[Direction.Up] = false;
                grid[x + 1, y + 1].IsEnergizedInDirection[Direction.Right] = false;
                grid[x + 1, y + 1].IsEnergizedInDirection[Direction.Down] = false;
                grid[x + 1, y + 1].IsEnergizedInDirection[Direction.Left] = false;
            }
        }

        return energized;
    }

    private static void PrintGrid(Tile[,] grid) {
        for (int x = 0; x < grid.GetLength(0); x++) {
            for (int y = 0; y < grid.GetLength(1); y++) {
                Console.Write(grid[x, y].ToChar(withBeams: true));
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

public class Tile {
    
    public Dictionary<Direction, bool> IsEnergizedInDirection { get; } = new();
    public bool IsEnergized => IsEnergizedInDirection.Any(kvp => kvp.Value);

    public MirrorType MirrorType { get; } = MirrorType.End;
    
    public Tile(char c) {
        IsEnergizedInDirection[Direction.Up] = false;
        IsEnergizedInDirection[Direction.Right] = false;
        IsEnergizedInDirection[Direction.Down] = false;
        IsEnergizedInDirection[Direction.Left] = false;
        MirrorType = c switch {
            '.' => MirrorType.None,
            '\\' => MirrorType.TopLeft,
            '/' => MirrorType.TopRight,
            '-' => MirrorType.SplitHor,
            '|' => MirrorType.SplitVert,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, "Unknown char")
        };
    }

    private Tile() {
        IsEnergizedInDirection[Direction.Up] = true;
        IsEnergizedInDirection[Direction.Right] = true;
        IsEnergizedInDirection[Direction.Down] = true;
        IsEnergizedInDirection[Direction.Left] = true;
    }

    public static Tile EndTile { get; } = new Tile();

    public (Direction a, bool split) GetNewDir(Direction direction) {
        Debug.Assert(MirrorType != MirrorType.End, "MirrorType == MirrorType.End when GetNewDir called!");
        switch (MirrorType, direction) {
            case (MirrorType.None,_):
            case (MirrorType.SplitHor, Direction.Left):
            case (MirrorType.SplitHor, Direction.Right):
            case (MirrorType.SplitVert, Direction.Up):
            case (MirrorType.SplitVert, Direction.Down):
                return (direction, false);
            
            case (MirrorType.SplitHor, _):
                return (Direction.Right, true);
            case (MirrorType.SplitVert, _):
                return (Direction.Down, true);
            
            case (MirrorType.TopLeft, Direction.Right):
                return (Direction.Down, false);
            case (MirrorType.TopLeft, Direction.Up):
                return (Direction.Left, false);
            case (MirrorType.TopLeft, Direction.Down):
                return (Direction.Right, false);
            case (MirrorType.TopLeft, Direction.Left):
                return (Direction.Up, false);
            
            case (MirrorType.TopRight, Direction.Right):
                return (Direction.Up, false);
            case (MirrorType.TopRight, Direction.Up):
                return (Direction.Right, false);
            case (MirrorType.TopRight, Direction.Down):
                return (Direction.Left, false);
            case (MirrorType.TopRight, Direction.Left):
                return (Direction.Down, false);
            
            default:
                Console.WriteLine($"Missed combination {MirrorType}, {direction}");
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public char ToChar(bool withBeams) {
        if (withBeams && IsEnergized) {
            if (IsEnergizedInDirection.Count(kvp => kvp.Value == true) > 1) {
                return (char)('0' + IsEnergizedInDirection.Count);
            }

            return (IsEnergizedInDirection[Direction.Right], IsEnergizedInDirection[Direction.Down],
                IsEnergizedInDirection[Direction.Left], IsEnergizedInDirection[Direction.Up]) switch {
                (true, false, false, false) => '>',
                (false, true, false, false) => 'v',
                (false, false, true, false) => '<',
                (false, false, false, true) => '^',
                (_, _, _, _) => throw new ArgumentOutOfRangeException()
            };
        }
        return MirrorType switch {
            MirrorType.None => '.',
            MirrorType.TopLeft => '\\',
            MirrorType.TopRight => '/',
            MirrorType.SplitHor => '-',
            MirrorType.SplitVert => '|',
            MirrorType.End => 'X',
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public enum MirrorType {
    None,
    TopLeft,
    TopRight,
    SplitHor,
    SplitVert,
    End,
}

public enum Direction {
    Up,
    Right,
    Down,
    Left,
}

