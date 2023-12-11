using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d11y2023; 

public static class Day11 {
    private const long ExpectedResultTest1 = 374;
    private const long ExpectedResultTest2 = 8410;
    private const string InputFileName = "inputDay11.txt";
    private const string TestFileName = "testInputDay11.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 0; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 0; // For ensuring it stays correct, once the actual result is known
    
    private const string Success = "✅";
    private const string Fail = "❌";

    public static void Main(string[] args) {
        TestRun();

        Stopwatch sw = new();
        sw.Start();
        Solve(InputFileName, out var result1, 2);
        Solve(InputFileName, out var result2, 1000000);
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
        Solve(TestFileName, out var resultTest1, 2);
        Solve(TestFileName, out var resultTest2, 100);
        PrintResult(resultTest1, ExpectedResultTest1, 1, true);
        if (Test2Started)
            PrintResult(resultTest2, ExpectedResultTest2, 2, true);
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, int growthFactor) {
        result1 = 0; 
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var initialWidth = allLines[0].Length;
        var initialHeight = allLines.Count; // .Length;

        var universe = new List<List<Tile>>();
        var galaxies = new List<Tile>();
        
        var rowsToDouble = new List<int>();
        var colsToAdd = new List<int>();
        for (int x = 0; x < initialWidth; x++) {
            colsToAdd.Add(x);
        }
        
        for (int y = 0; y < initialHeight; y++) {
            var line = allLines[y];
            universe.Add(new List<Tile>());
            for (int x = 0; x < initialWidth; x++) {
                var c = line[x];
                switch (c) {
                    case '.':
                        universe[y].Add(new Tile(false));
                        break;
                    case '#':
                        universe[y].Add(new Tile(true));
                        galaxies.Add(universe[y][x]);
                        colsToAdd.Remove(x);
                        break;
                    default:
                        throw new Exception($"Unexpected char {c} at position {x},{y}");
                }
            }
            var lineExpand = !line.Contains('#');
            if (lineExpand) {
                rowsToDouble.Add(y);
            }
        }
        Console.WriteLine($"Galaxies: {galaxies.Count}, rows to double: {string.Join(", ", rowsToDouble)} ({rowsToDouble.Count}), cols to double: {string.Join(", ", colsToAdd)} ({colsToAdd.Count})");
        
        // let galaxy grow
        // var insertedRows = 0;
        // foreach (var y in rowsToDouble) {
        //     universe.Insert(y + insertedRows, new List<Tile>());
        //     for (int x = 0; x < initialWidth; x++) {
        //         universe[y + insertedRows].Add(new Tile(false));
        //     }
        //     insertedRows++;
        // }
        // var insertedCols = 0;
        // foreach (var x in colsToDouble) {
        //     foreach (var row in universe) {
        //         row.Insert(x + insertedCols, new Tile(false));
        //     }
        //     insertedCols++;
        // }
        
        var width = universe[0].Count;
        var height = universe.Count;

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var actualX = colsToAdd.Count(i => i < x) * (growthFactor - 1) + x; // -1 because we already have the initial row
                var actualY = rowsToDouble.Count(i => i < y) * (growthFactor - 1) + y;
                universe[y][x].Position = (actualX, actualY);
            }
        }
        
        // PrintUniverse(universe);
            
        var galaxyPairs = new List<(Tile a, Tile b)>();
        for (int i = 0; i < galaxies.Count; i++) {
            for (int j = i + 1; j < galaxies.Count; j++) {
                galaxyPairs.Add((galaxies[i], galaxies[j]));
            }
        }

        Console.WriteLine($"Galaxies: {galaxies.Count}, pairs: {galaxyPairs.Count}");

        foreach (var galaxyPair in galaxyPairs) {
            Debug.Assert(galaxyPair.a.IsGalaxy && galaxyPair.b.IsGalaxy, "Not a galaxy pair!");
            Debug.Assert(galaxyPair.a.Position != galaxyPair.b.Position, "Galaxy pair has same position!");
            var distance = GetDistance(galaxyPair.a, galaxyPair.b);
            result1 += distance;
        }
    }

    private static void PrintUniverse(List<List<Tile>> universe) {
        var width = universe[0].Count;
        var height = universe.Count;
        for (int y = 0; y < height; y++) {
            var line = "";
            for (int x = 0; x < width; x++) {
                line += universe[y][x].IsGalaxy ? "#" : ".";
            }
            Console.WriteLine(line);
        }
    }

    private static int GetDistance(Tile galaxyPairA, Tile galaxyPairB) {
        var (x1, y1) = galaxyPairA.Position;
        var (x2, y2) = galaxyPairB.Position;
        return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }
}

internal class Tile {
    
    public bool IsGalaxy { get; }
    
    public Tile(bool isGalaxy) {
        IsGalaxy = isGalaxy;
    }
    
    public (int x, int y) Position { get; internal set; }

}
