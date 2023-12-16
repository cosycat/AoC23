using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace d14y2023; 

public static class Day14 {
    private const long ExpectedResultTest1 = 136;
    private const long ExpectedResultTest2 = 64;
    private const string InputFileName = "inputDay14.txt";
    private const string TestFileName = "testInputDay14.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 113525; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 101292; // For ensuring it stays correct, once the actual result is known
    
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
        // Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        // Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0; 
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var width = allLines[0].Length;
        var height = allLines.Count; // .Length;

        var grid = new Tile[height, width];

        // var rowsString = allLines.ToList();
        // var colsString = new List<string>();
        //
        // for (int x = 0; x < width; x++) {
        //     colsString.Add("");
        // }
        
        // Process input char by char
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                grid[y, x] = new Tile {
                    Type = c switch {
                        'O' => TileType.Rolling,
                        '#' => TileType.Fixed,
                        '.' => TileType.Empty,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                };
                // colsString[x] += c;
            }
        }

        // RollGrid(grid, Direction.N);
        // Console.WriteLine("N");
        // PrintGrid(grid);
        // result1 = CountWeight(grid);
        // // Debug.Assert(result1 == ExpectedResultTest1);
        // RollGrid(grid, Direction.W);
        // Console.WriteLine("W");
        // PrintGrid(grid);
        // RollGrid(grid, Direction.S);
        // Console.WriteLine("S");
        // PrintGrid(grid);
        // RollGrid(grid, Direction.E);
        // Console.WriteLine("E");
        // PrintGrid(grid);
        
        grid = RollGridCycle(grid, 1000);
        
        result2 = CountWeight(grid);
    }

    private static Tile[,] RollGridCycle(Tile[,] grid, int cycles) {
        var numStoredGrids = 1000;
        var prevGrids = new List<(Tile[,] grid, long weight)>();
        var startWeight = CountWeight(grid);
        
        var originalGrid = new Tile[grid.GetLength(0), grid.GetLength(1)];
        CopyGrid(grid, originalGrid);
        
        for (int i = 0; i < cycles; i++) {
            if (i % 100000 == 0)
                Console.WriteLine($"Cycle {i}");
            RollGrid(grid, Direction.N);
            RollGrid(grid, Direction.W);
            RollGrid(grid, Direction.S);
            RollGrid(grid, Direction.E);
            
            
            var changesToOriginal = CompareChanges(originalGrid, grid);
            if (changesToOriginal.Count == 0) {
                Console.WriteLine($"Cycle {i} is the same as the original one!");
                return grid;
            }

            for (int j = 0; j < prevGrids.Count; j++) {
                var gridToCompare = prevGrids[j].grid;
                var prevGridChanges = CompareChanges(gridToCompare, grid);
                var hasPrevWeightChanged = prevGrids[j].weight != CountWeight(grid);
                if (prevGridChanges.Count == 0) {
                    Console.WriteLine($"Cycle {i} is the same as cycle {i - j}! weight: {CountWeight(grid)}, prev weight: {prevGrids[j].weight}");
                    prevGrids.Select((g, i) => g.weight).Distinct().ToList().ForEach(w => Console.WriteLine($"   weight {w}: {prevGrids.Count(g => g.weight == w)}"));
                    return grid;
                }
            }
            
            // var gridCopy = new Tile[grid.GetLength(0), grid.GetLength(1)];
            // CopyGrid(grid, gridCopy);
            // prevGrids.Add((gridCopy, CountWeight(grid)));
            
            // // multiple changes
            // for (int j = 1; j < prevGrids.Count; j++) {
            //     var prevGridChanges = CompareChanges(prevGrids[j - 1].grid, grid);
            //     var hasPrevWeightChanged = prevGrids[j - 1].weight != CountWeight(grid);
            //     var weight = CountWeight(grid);
            //     if (false && prevGridChanges.Count == 0) {
            //         var cycleLength = i - j;
            //         var cycleIndex = cycles % cycleLength;
            //         Console.WriteLine($"Cycle {i} is the same as cycle {i - j}! Cycle length: {cycleLength}, cycle index: {cycleIndex}");
            //         Console.WriteLine($"weight: {weight}, prev weight: {prevGrids[j - 1].weight}");
            //         // Console.WriteLine($"All weights: {string.Join(", ", prevGrids.Select(g => g.weight))}");
            //         return grid;
            //     }
            //     else if (i % 1000 == 0) {
            //         // Console.WriteLine($"   Diffences: {string.Join(", ", prevGridChanges)}");
            //     }
            //     CopyGrid(prevGrids[j].grid, prevGrids[j - 1].grid);
            //     // prevGrids[j - 1] = (prevGrids[j].grid, weight);
            // }

            // if (i % 100000 == 0) {
            //     PrintGrid(prevGrids[^1].grid);
            //     Console.WriteLine($"   Changes: {string.Join(", ", changes)}");
            //     PrintGrid(grid);
            // }

            // CopyGrid(grid, prevGrids[^1].grid);
            
        }

        return grid;
    }

    private static void CopyGrid(Tile[,] from, Tile[,] to) { 
        var width = from.GetLength(1);
        var height = from.GetLength(0);
        Debug.Assert(width == to.GetLength(1));
        Debug.Assert(height == to.GetLength(0));
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) { 
                to[y, x] = new Tile {
                    Type = from[y, x].Type
                };
            }
        }
    }

    private static List<(int x, int y)> CompareChanges(Tile[,] prevGrid, Tile[,] grid) {
        var changes = new List<(int x, int y)>();
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) { 
                if (prevGrid[y, x].Type != grid[y, x].Type) {
                    changes.Add((x, y));
                }
            }
        }

        return changes;
    }


    private static (Tile[,] changedGrid, List<int> changedLines) RollGrid(Tile[,] grid, Direction dir) {
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        
        var firstDim = dir == Direction.N || dir == Direction.S ? width : height;
        var secondDim = dir == Direction.N || dir == Direction.S ? height : width;
        var startLine = dir == Direction.W || dir == Direction.N ? 0 : secondDim - 1;
        var endLine = dir == Direction.W || dir == Direction.N ? secondDim - 1 : 0;
        var step = dir == Direction.N || dir == Direction.W ? 1 : -1;
        
        var reversed = dir == Direction.S || dir == Direction.E;

        // Console.WriteLine($"Rolling grid {firstDim}x{secondDim} from {startLine} to {endLine} with step {step}");

        var changedLines = new List<int>();
        for (int i = 0; i < firstDim; i++) {
            // Console.WriteLine($"line {i}:");
            var line = new Tile[secondDim];
            for (int j = startLine; j >= 0 && j < secondDim; j += step) {
                line[j] = dir == Direction.N || dir == Direction.S ? grid[j, i] : grid[i, j];
            }

            if (RollLineToLeft(line, reversed)) {
                changedLines.Add(i);
            }
            for (int j = startLine; j != endLine; j += step) {
                if (dir == Direction.N || dir == Direction.S)
                    grid[j, i] = line[j];
                else
                    grid[i, j] = line[j];
            }
        }

        return (grid, changedLines);
    }

    private static bool RollLineToLeft(Tile[] line, bool reversed) {
        Debug.Assert(line.All(t => t != null), $"Line contains nulls at {string.Join(", ", line.ToList().Select((t, i) => (t, i)).Where(t => t.t == null).Select(t => t.i))}");
        // Console.WriteLine($"Rolling line {line.Length}");
        var changed = false;
        for (int x = 0; x < line.Length; x++) {
            var currX = reversed ? line.Length - x - 1 : x;
            var currTile = line[currX];
            if (currTile.Type != TileType.Empty) continue;
            var nextX = currX;
            if (reversed) {
                while (nextX >= 0 && line[nextX].Type == TileType.Empty) {
                    nextX--;
                }
            }
            else {
                while (nextX < line.Length && line[nextX].Type == TileType.Empty) {
                    nextX++;
                }
            }

            if (nextX >= line.Length || nextX < 0) continue;
            if (line[nextX].Type == TileType.Fixed) continue;
            Debug.Assert(line[nextX].Type == TileType.Rolling);
            changed = true;
            line[currX].Type = TileType.Rolling;
            line[nextX].Type = TileType.Empty;
        }
        
        return changed;
    }

    private static long CountWeight(Tile[,] grid) {
        var result = 0L;
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (grid[y, x].Type == TileType.Rolling)
                    result += height - y;
            }
        }

        return result;
    }

    private static void PrintGrid(Tile[,] grid) {
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var c = grid[y, x].Type switch {
                    TileType.Empty => '.',
                    TileType.Rolling => 'O',
                    TileType.Fixed => '#',
                    _ => throw new ArgumentOutOfRangeException()
                };
                Console.Write(c);
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }
}

class Tile {
    public TileType Type { get; set; }
}

enum TileType {
    Empty,
    Rolling,
    Fixed
}

enum Direction {
    N,
    E,
    S,
    W
}
