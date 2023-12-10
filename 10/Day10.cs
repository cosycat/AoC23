using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d10y2023; 

public static class Day10 {
    private const long ExpectedResultTest1 = 8;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay10.txt";
    private const string TestFileName = "testInputDay10.txt";
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
        var width = allLines[0].Length;
        var height = allLines.Count; // .Length;

        var grid = new Tile[height, width];
        
        Tile startTile = null;
        
        // Process input char by char
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                switch (c) {
                    case '.':
                        grid[y, x] = null;
                        break;
                    case '|':
                        grid[y, x] = new Tile(x, y, Direction.Up, Direction.Down);
                        break;
                    case '-':
                        grid[y, x] = new Tile(x, y, Direction.Left, Direction.Right);
                        break;
                    case 'L':
                        grid[y, x] = new Tile(x, y, Direction.Right, Direction.Up);
                        break;
                    case '7':
                        grid[y, x] = new Tile(x, y, Direction.Left, Direction.Down);
                        break;
                    case 'J':
                        grid[y, x] = new Tile(x, y, Direction.Up, Direction.Left);
                        break;
                    case 'F':
                        grid[y, x] = new Tile(x, y, Direction.Down, Direction.Right);
                        break; 
                    case 'S':
                        grid[y, x] = new Tile(x, y);
                        startTile = grid[y, x];
                        break;
                    default:
                        Debug.Fail($"Unknown char {c}!");
                        break;
                }
            }
        }
        
        Debug.Assert(startTile != null, "No start tile found!");
        
        startTile.initStartTile(grid);
        
        var currTileA = startTile.GetTileInDirection(grid, startTile.DirA);
        var prevTileA = startTile;
        var currTileB = startTile.GetTileInDirection(grid, startTile.DirB);
        var prevTileB = startTile;

        result1 = 1;
        while (currTileA != currTileB) {
            Debug.Assert(currTileA != null, $"Tile A is null! from {prevTileA.X},{prevTileA.Y}");
            Debug.Assert(currTileB != null, $"Tile B is null! from {prevTileB.X},{prevTileB.Y}");
            var newTileA = currTileA.GetNextTile(grid, prevTileA);
            prevTileA = currTileA;
            currTileA = newTileA;
            var newTileB = currTileB.GetNextTile(grid, prevTileB);
            prevTileB = currTileB;
            currTileB = newTileB;
            result1++;
        }
        
    }
    
}

public enum Direction {
    Up,
    Down,
    Left,
    Right
}

internal class Tile {
    public int X { get; }
    public int Y { get; }
    public Direction DirA { get; private set; }
    public Direction DirB { get; private set; }
    
    public Tile(int x, int y, Direction dirA, Direction dirB) {
        X = x;
        Y = y;
        DirA = dirA;
        DirB = dirB;
    }

    public Tile(int x, int y) {
        X = x;
        Y = y;
    }
    
    public void initStartTile(Tile[,] grid) {
        var setA = false;
        for (int i = 0; i < 4; i++) {
            var dir = (Direction) i;
            var tileInDirection = GetTileInDirection(grid, dir);
            if (tileInDirection == null) continue;
            if (tileInDirection.GetTileInDirection(grid, tileInDirection.DirA) == this || tileInDirection.GetTileInDirection(grid, tileInDirection.DirB) == this) {
                if (setA) {
                    DirB = dir;
                    return;
                }
                else {
                    DirA = dir;
                    setA = true;
                }
            }
        }
        Debug.Fail($"Could not set start tile directions!");
    }
    
    public Tile GetTileInDirection(Tile[,] grid, Direction dir) {
        var x = X + dir switch {
            Direction.Left => -1,
            Direction.Right => 1,
            _ => 0
        };
        var y = Y + dir switch {
            Direction.Up => -1,
            Direction.Down => 1,
            _ => 0
        };
        if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1)) {
            return grid[y, x];
        }
        else {
            return null;
        }
    }

    public Tile GetNextTile(Tile[,] grid, Tile comingFrom) {
        if (comingFrom == GetTileInDirection(grid, DirA)) {
            return GetTileInDirection(grid, DirB);
        }
        else {
            Debug.Assert(comingFrom == GetTileInDirection(grid, DirB), $"Tile {comingFrom.X},{comingFrom.Y} is not in direction {DirB}!");
            return GetTileInDirection(grid, DirA);
        }
    }
}
