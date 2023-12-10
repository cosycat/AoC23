using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d10y2023; 

public static class Day10 {
    private const long ExpectedResultTest1 = 80;
    private const long ExpectedResultTest2 = 10;
    private const string InputFileName = "inputDay10.txt";
    private const string TestFileName = "testInputDay10.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 6951; // For ensuring it stays correct, once the actual result is known
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
        
        startTile.InitStartTile(grid);
        startTile.IsPartOfLoop = true;
        
        var currTileA = startTile.GetTileInDirection(grid, startTile.DirA);
        var prevTileA = startTile;
        var currTileB = startTile.GetTileInDirection(grid, startTile.DirB);
        var prevTileB = startTile;

        result1 = 1;
        while (currTileA != currTileB) {
            if (currTileA == null || currTileB == null) {
                PrintGrid(grid, width, height, allLines, false, new List<(int x, int y)> {(prevTileA.X, prevTileA.Y), (prevTileB.X, prevTileB.Y)});
            }
            Debug.Assert(currTileA != null, $"Tile A is null! from {prevTileA.X},{prevTileA.Y}");
            Debug.Assert(currTileB != null, $"Tile B is null! from {prevTileB.X},{prevTileB.Y}");
            currTileA.IsPartOfLoop = true;
            currTileB.IsPartOfLoop = true;
            var newTileA = currTileA.GetNextTile(grid, prevTileA);
            prevTileA = currTileA;
            currTileA = newTileA;
            var newTileB = currTileB.GetNextTile(grid, prevTileB);
            prevTileB = currTileB;
            currTileB = newTileB;
            result1++;
        }
        currTileA.IsPartOfLoop = true;
        
        PrintGrid(grid, width, height);

        var expandedWidth = width * 2;
        var expandedHeight = height * 2;
        var expandedGrid = new Tile[expandedHeight, expandedWidth];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var currTile = grid[y, x];
                if (currTile == null) {
                    expandedGrid[y * 2, x * 2] = new Tile(x * 2, y * 2, Direction.None, Direction.None, true);
                    expandedGrid[y * 2, x * 2].IsNull = true;
                    expandedGrid[y * 2, x * 2 + 1] = new Tile(x * 2 + 1, y * 2, Direction.None, Direction.None, false);
                    expandedGrid[y * 2, x * 2 + 1].IsNull = true;
                    expandedGrid[y * 2 + 1, x * 2] = new Tile(x * 2, y * 2 + 1, Direction.None, Direction.None, false);
                    expandedGrid[y * 2 + 1, x * 2].IsNull = true;
                    expandedGrid[y * 2 + 1, x * 2 + 1] = new Tile(x * 2 + 1, y * 2 + 1, Direction.None, Direction.None, false);
                    expandedGrid[y * 2 + 1, x * 2 + 1].IsNull = true;
                    continue;
                }
                
                expandedGrid[y * 2, x * 2] = currTile;
                expandedGrid[y * 2, x * 2].IsPartOfOriginalGrid = true;
                expandedGrid[y * 2, x * 2 + 1] = new Tile(x * 2 + 1, y * 2, Direction.None, Direction.None, false);
                var isLeftConnected = currTile.DirA == Direction.Right || currTile.DirB == Direction.Right;
                var rightTile = currTile.GetTileInDirection(grid, Direction.Right);
                var isRightConnected = rightTile != null && (rightTile.DirA == Direction.Left || rightTile.DirB == Direction.Left);
                if (isRightConnected && isLeftConnected && currTile.IsPartOfLoop) {
                    expandedGrid[y * 2, x * 2 + 1].DirA = Direction.Right;
                    expandedGrid[y * 2, x * 2 + 1].DirB = Direction.Left;
                    expandedGrid[y * 2, x * 2 + 1].IsPartOfLoop = true;
                }
                else {
                    expandedGrid[y * 2, x * 2 + 1].IsNull = true;
                }

                expandedGrid[y * 2 + 1, x * 2] = new Tile(x * 2, y * 2 + 1, Direction.None, Direction.None, false);
                var isUpConnected = currTile.DirA == Direction.Down || currTile.DirB == Direction.Down;
                var downTile = currTile.GetTileInDirection(grid, Direction.Down);
                var isDownConnected = downTile != null && (downTile.DirA == Direction.Up || downTile.DirB == Direction.Up);
                if (isUpConnected && isDownConnected && currTile.IsPartOfLoop) {
                    expandedGrid[y * 2 + 1, x * 2].DirA = Direction.Down;
                    expandedGrid[y * 2 + 1, x * 2].DirB = Direction.Up;
                    expandedGrid[y * 2 + 1, x * 2].IsPartOfLoop = true;
                }
                else {
                    expandedGrid[y * 2 + 1, x * 2].IsNull = true;
                }
                expandedGrid[y * 2 + 1, x * 2 + 1] = new Tile(x * 2 + 1, y * 2 + 1, Direction.None, Direction.None, false);
                expandedGrid[y * 2 + 1, x * 2 + 1].IsNull = true;
                
                currTile.X = x * 2;
                currTile.Y = y * 2;
            }
        }

        var IsConnectedToOutsideToCheck = new Stack<Tile>();
        for (int y = 0; y < expandedHeight; y++) {
            for (int x = 0; x < expandedWidth; x++) {
                var tile = expandedGrid[y, x];
                Debug.Assert(tile != null, $"Tile at {x},{y} is null in expanded grid! width: {expandedWidth}, height: {expandedHeight}");
                if (tile.IsPartOfLoop) continue;
                var isOnEdge = x == 0 || x == expandedWidth - 1 || y == 0 || y == expandedHeight - 1;
                if (isOnEdge) {
                    IsConnectedToOutsideToCheck.Push(tile);
                    tile.IsConnectedToOutside = true;
                }
            }
        }

        PrintGrid(expandedGrid, expandedWidth, expandedHeight, null, false);
        Console.WriteLine();

        while (IsConnectedToOutsideToCheck.Count > 0) {
            var nextTile = IsConnectedToOutsideToCheck.Pop();
            Debug.Assert(nextTile != null, "Next tile is null!");
            for (int i = 0; i < 4; i++) {
                var dir = (Direction) i;
                var tileInDirection = nextTile.GetTileInDirection(expandedGrid, dir);
                if (tileInDirection == null) continue;
                // Debug.Assert(tileInDirection != null, $"Tile in direction {dir} of {nextTile.X},{nextTile.Y} is null!"); // can be null if it's outside the grid
                if (tileInDirection.IsConnectedToOutside) continue;
                if (tileInDirection.IsPartOfLoop) continue;
                tileInDirection.IsConnectedToOutside = true;
                IsConnectedToOutsideToCheck.Push(tileInDirection);
            }
        }
        
        for (int y = 0; y < expandedHeight; y++) {
            for (int x = 0; x < expandedWidth; x++) {
                var tile = expandedGrid[y, x];
                Debug.Assert(tile != null, $"Tile at {x},{y} is null in expanded grid!");
                if (tile.IsPartOfLoop) continue;
                if (tile.IsConnectedToOutside) continue;
                if (!tile.IsPartOfOriginalGrid) continue;
                result2++;
            }
        }
        
        PrintGrid(expandedGrid, expandedWidth, expandedHeight, null, false);


    }

    private static void PrintGrid(Tile[,] grid, int width, int height, List<string> allLines = null, bool skipUneven = false, List<(int x, int y)> specialFields = null) {
        for (int y = 0; y < height; y++) {
            var line = "";
            for (int x = 0; x < width; x++) {
                if (specialFields != null && specialFields.IndexOf((x, y)) != -1) {
                    line += (char)((grid[y, x] == null ? 'a' : 'A') + specialFields.IndexOf((x, y)));
                    continue;
                }
                
                var tile = grid[y, x];
                if (tile == null) {
                    line += ".";
                }
                else {
                    line += tile.AsChar();
                }
            }
            if (allLines != null && (!skipUneven || y % 2 == 0)) {
                line += " " + allLines[y];
            }
            Console.WriteLine(line);
        }
    }
}

public enum Direction {
    Up,
    Down,
    Left,
    Right,
    None,
}

internal class Tile {
    public int X { get; set; }
    public int Y { get; set; }
    public Direction DirA { get; set; }
    public Direction DirB { get; set; }
    
    
    public bool IsNull { get; set; }
    public bool IsPartOfLoop { get; set; } = false;
    public bool IsPartOfOriginalGrid { get; set; }
    public bool IsStartTile { get; }
    public bool IsConnectedToOutside { get; set; } = false;
    
    public Tile(int x, int y, Direction dirA, Direction dirB, bool isPartOfOriginalGrid = true) {
        X = x;
        Y = y;
        DirA = dirA;
        DirB = dirB;
        IsPartOfOriginalGrid = isPartOfOriginalGrid;
    }

    public Tile(int x, int y) {
        X = x;
        Y = y;
        IsPartOfOriginalGrid = true;
        IsStartTile = true;
    }
    
    public void InitStartTile(Tile[,] grid) {
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
    
    public Tile? GetTileInDirection(Tile[,] grid, Direction dir) {
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
        if (x >= 0 && x < grid.GetLength(1) && y >= 0 && y < grid.GetLength(0)) {
            return grid[y, x];
        }
        else {
            return null;
        }
    }

    public Tile GetNextTile(Tile[,] grid, Tile comingFrom) {
        if (comingFrom == GetTileInDirection(grid, DirA)) {
            return GetTileInDirection(grid, DirB)!;
        }
        else {
            Debug.Assert(comingFrom == GetTileInDirection(grid, DirB), $"Tile {comingFrom.X},{comingFrom.Y} is not in direction {DirB}!");
            return GetTileInDirection(grid, DirA)!;
        }
    }

    public string AsChar() {
        // if (IsNull) return ".";
        // if (!IsPartOfLoop) return "O";
        // if (IsStartTile) return "S";
        if (IsPartOfLoop)
            return (DirA, DirB) switch {
                (Direction.Up, Direction.Down) => "|",
                (Direction.Down, Direction.Up) => "|",
                (Direction.Left, Direction.Right) => "-",
                (Direction.Right, Direction.Left) => "-",
                (Direction.Right, Direction.Up) => "L",
                (Direction.Up, Direction.Right) => "L",
                (Direction.Left, Direction.Down) => "7",
                (Direction.Down, Direction.Left) => "7",
                (Direction.Up, Direction.Left) => "J",
                (Direction.Left, Direction.Up) => "J",
                (Direction.Down, Direction.Right) => "F",
                (Direction.Right, Direction.Down) => "F",
                _ => throw new Exception($"Unknown directions {DirA} and {DirB}!")
            };
        if (IsConnectedToOutside) return IsPartOfOriginalGrid ? "O" : "o";
        return "•";
    }
    
    public override string ToString() {
        return $"{X},{Y} ({DirA},{DirB}) {(IsPartOfLoop ? "O" : "o")} {(IsStartTile ? "S" : "")} {(IsPartOfOriginalGrid ? "" : "E")}";
    }
}
