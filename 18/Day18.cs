using System.Diagnostics;
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
        const string color = @"[0-9a-f]{2}";
        const string colorCode = $@"#(?<r>{color})(?<g>{color})(?<b>{color})";
        const string mainPattern = $@"(?<direction>[UDLR])\s(?<distance>\d+)\s\(#{colorCode}\)";
        // Regex for strings like "InputLine 1: 10,  2, 33,  4, 56, 78,  9"
        Console.WriteLine($"Regex: {mainPattern}");
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} does not match {mainMatch.Value}");
            var direction = mainMatch.Groups["direction"].Value;
            var distance = int.Parse(mainMatch.Groups["distance"].Value);
            var colorR = int.Parse(mainMatch.Groups["r"].Value);
            var colorG = int.Parse(mainMatch.Groups["g"].Value);
            var colorB = int.Parse(mainMatch.Groups["b"].Value);
        }

        
        
        
        
    }
    
}
