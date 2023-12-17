using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d17y2023; 

public static class Day17 {
    private const long ExpectedResultTest1 = 102;
    private const long ExpectedResultTest2 = 94;
    private const string InputFileName = "inputDay17.txt";
    private const string TestFileName = "testInputDay17.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 936; // For ensuring it stays correct, once the actual result is known
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
        
        var grid = new int[height, width];
        
        var maxHeatLoss = 0;
        var minHeatLoss = int.MaxValue;
        
        // Process input char by char
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                grid[y, x] = int.Parse($"{c}");
                maxHeatLoss = Math.Max(maxHeatLoss, grid[y, x]);
                minHeatLoss = Math.Min(minHeatLoss, grid[y, x]);
            }
        }

        var usedHeadLossToGuess = minHeatLoss;

        FindPath(ref result1, width, height, usedHeadLossToGuess, grid);
        FindPath(ref result2, width, height, usedHeadLossToGuess, grid, 4, 10);
        
    }

    private static void FindPath(ref long res, int width, int height, int usedHeadLossToGuess, int[,] grid, int minSteps = 1, int maxSteps = 3) {
        var currentPaths = new PriorityQueue<PathPosData, int>();
        
        var starPosRight = new PathPosData { X = 0, Y = 0, HeatLoss = 0, NextDirections = Directions.LeftRight };
        currentPaths.Enqueue(starPosRight, starPosRight.GetPriority(width, height, usedHeadLossToGuess));
        var starPosDown = new PathPosData { X = 0, Y = 0, HeatLoss = 0, NextDirections = Directions.UpDown };
        currentPaths.Enqueue(starPosDown, starPosDown.GetPriority(width, height, usedHeadLossToGuess));
        
        var visited = new Dictionary<(int x, int y, Directions nextDirs), (int heatLoss, PathPosData prevTile)>();

        while (currentPaths.Count > 0) {
            var currentPathPos = currentPaths.Dequeue();
            // if (visited.TryGetValue((currentPathPos.X, currentPathPos.Y, currentPathPos.NextDirections), out var visitedData)) {
            //     if (visitedData.heatLoss <= currentPathPos.HeatLoss) {
            //         continue;
            //     }
            // }
            // visited[(currentPathPos.X, currentPathPos.Y, currentPathPos.NextDirections)] = (currentPathPos.HeatLoss, currentPathPos);
            
            // Console.WriteLine($"Current path: {currentPathPos.X}, {currentPathPos.Y}, {currentPathPos.HeatLoss}, {currentPathPos.NextDirections}");
            if (currentPathPos.X == width - 1 && currentPathPos.Y == height - 1) {
                res = currentPathPos.HeatLoss;
                break;
            }
            foreach (var nextPathPos in GetNextTiles(grid, currentPathPos, minSteps, maxSteps)) {
                if (visited.TryGetValue((nextPathPos.X, nextPathPos.Y, nextPathPos.NextDirections), out var visitedData2)) {
                    if (visitedData2.heatLoss <= nextPathPos.HeatLoss) {
                        continue;
                    }
                }
                visited[(nextPathPos.X, nextPathPos.Y, nextPathPos.NextDirections)] = (nextPathPos.HeatLoss, currentPathPos);
                currentPaths.Enqueue(nextPathPos, nextPathPos.GetPriority(width, height, usedHeadLossToGuess));
            }
        }

        Console.WriteLine($"Result 1: {res}");

        // foreach (var kvp in visited) {
        //     var tile = kvp.Key;
        //     var prevTileData = kvp.Value.prevTile;
        //     var heatLoss = kvp.Value.heatLoss;
        //     Console.WriteLine($"{tile.x}, {tile.y} {heatLoss} => {prevTileData.X}, {prevTileData.Y}, {prevTileData.HeatLoss}, {prevTileData.NextDirections}");
        // }

        // Console.WriteLine();
        // var prevPos = visited[(width - 1, height - 1, Directions.LeftRight)].prevTile;
        // Console.WriteLine($"Last Tile from: {prevPos.X}, {prevPos.Y}, {prevPos.HeatLoss}, {prevPos.NextDirections}");
        // Console.WriteLine();
        
        PrintFoundPath(visited, width, height, grid);
    }

    private static void PrintFoundPath(Dictionary<(int x, int y, Directions nextDirs), (int heatLoss, PathPosData prevTile)> visited, int width, int height, int[,] grid) {
        var path = new List<PathPosData>();
        var currentPos = visited.TryGetValue((width - 1, height - 1, Directions.LeftRight), out var data) 
            ? data.prevTile : visited.TryGetValue((width - 1, height - 1, Directions.UpDown), out data) ? data.prevTile : throw new ArgumentOutOfRangeException();
        while (currentPos.X != 0 || currentPos.Y != 0) {
            path.Add(currentPos);
            currentPos = visited.TryGetValue((currentPos.X, currentPos.Y, currentPos.NextDirections), out data) ? data.prevTile : throw new ArgumentOutOfRangeException();
            // Console.WriteLine($"{currentPos.X}, {currentPos.Y}, {currentPos.HeatLoss}, {currentPos.NextDirections}");
        }
        path.Reverse();
        foreach (var pathPosData in path) {
            Console.WriteLine($"{pathPosData.X}, {pathPosData.Y}, {pathPosData.HeatLoss}, {pathPosData.NextDirections}");
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var index = path.FindIndex(pathPos => pathPos.X == x && pathPos.Y == y);
                if (index > -1) {
                    if (index == 0) {
                        Console.Write($"S{grid[y, x]}");
                        continue;
                    }
                    var from = path[index - 1];
                    var sign = from.X < x ? "←" : from.X > x ? "→" : from.Y < y ? "↑" : "↓";
                    Console.Write($"{sign}{grid[y, x]}");
                } else {
                    Console.Write("  ");
                }
            }
            Console.WriteLine();
        }
    }

    private static List<PathPosData> GetNextTiles(int[,] grid, PathPosData currentPathPos, int minSteps, int maxSteps) {
        var x = currentPathPos.X;
        var y = currentPathPos.Y;
        var currentHeatLoss = currentPathPos.HeatLoss;
        var directions = currentPathPos.NextDirections;
        
        var nextPos = new List<PathPosData>(6);
        switch (directions) {
            case Directions.LeftRight:
                var addedHeatLoss = 0;
                for (int i = 1; i < minSteps; i++) {
                    if (x - i < 0)
                        break;
                    addedHeatLoss += grid[y, x - i];
                }
                for (int newX = x - minSteps; newX >= Math.Max(0, x - maxSteps); newX--) {
                    addedHeatLoss += grid[y, newX];
                    nextPos.Add(new PathPosData(){X = newX, Y = y, HeatLoss = currentHeatLoss + addedHeatLoss, NextDirections = Directions.UpDown});
                }
                addedHeatLoss = 0;
                for (int i = 1; i < minSteps; i++) {
                    if (x + i >= grid.GetLength(1))
                        break;
                    addedHeatLoss += grid[y, x + i];
                }
                for (int newX = x + minSteps; newX <= Math.Min(x + maxSteps, grid.GetLength(1) - 1); newX++) {
                    addedHeatLoss += grid[y, newX];
                    nextPos.Add(new PathPosData(){X = newX, Y = y, HeatLoss = currentHeatLoss + addedHeatLoss, NextDirections = Directions.UpDown});
                }
                break;
            case Directions.UpDown:
                addedHeatLoss = 0;
                for (int i = 1; i < minSteps; i++) {
                    if (y - i < 0)
                        break;
                    addedHeatLoss += grid[y - i, x];
                }
                for (int newY = y - minSteps; newY >= Math.Max(0, y - maxSteps); newY--) {
                    addedHeatLoss += grid[newY, x];
                    nextPos.Add(new PathPosData {X = x, Y = newY, HeatLoss = currentHeatLoss + addedHeatLoss, NextDirections = Directions.LeftRight});
                }
                addedHeatLoss = 0;
                for (int i = 1; i < minSteps; i++) {
                    if (y + i >= grid.GetLength(0))
                        break;
                    addedHeatLoss += grid[y + i, x];
                }
                for (int newY = y + minSteps; newY <= Math.Min(y + maxSteps, grid.GetLength(0) - 1); newY++) {
                    addedHeatLoss += grid[newY, x];
                    nextPos.Add(new PathPosData {X = x, Y = newY, HeatLoss = currentHeatLoss + addedHeatLoss, NextDirections = Directions.LeftRight});
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(directions), directions, null);
        }

        return nextPos;
    }
}

public struct PathPosData {
    public int X;
    public int Y;
    public int HeatLoss;
    public Directions NextDirections;
    public int GetPriority(int width, int height, int maxHeatLoss) => HeatLoss;// + (width - X) * maxHeatLoss + (height - Y) * maxHeatLoss;
}

public enum Directions {
    UpDown,
    LeftRight,
}
