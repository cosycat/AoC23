using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d13y2023; 

public static class Day13 {
    private const long ExpectedResultTest1 = 0; // TODO replace
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay13.txt";
    private const string TestFileName = "testInputDay13.txt";
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

        var puzzles = File.ReadAllText(inputFileName).Split("\n\n");

        foreach (var puzzle in puzzles) {
            var lines = puzzle.Split("\n");
            var width = lines[0].Length;
            var height = lines.Length;
            var board = new Fields[height,width];
            //init
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    var c = lines[y][x];
                    board[y, x] = c switch {
                        '.' => Fields.Ash,
                        '#' => Fields.Rock,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }
            
            
        }
        
    }
    
}

enum Fields {
    Rock,
    Ash
}
