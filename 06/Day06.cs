using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d06y2023; 

public static class Day06 {
    private const long ExpectedResultTest1 = 288;
    private const long ExpectedResultTest2 = 71503;
    private const string InputFileName = "inputDay06.txt";
    private const string TestFileName = "testInputDay06.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 345015; // For ensuring it stays correct, once the actual result is known
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
        result1 = 1;
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");

        // Process input line by line with regex
        const string singleInputName = "SingleInput";
        const string singleInputPattern = @"\d+";
        const string mainPattern = $@"\w+:(?:\s*(?'{singleInputName}'{singleInputPattern}),?)+";
        // Regex for strings like "InputLine 1: 10,  2, 33,  4, 56, 78,  9"
        var times = Regex.Match(allLines[0], mainPattern).Groups[singleInputName].Captures.Select(c => int.Parse(c.Value)).ToList();
        var distances = Regex.Match(allLines[1], mainPattern).Groups[singleInputName].Captures.Select(c => int.Parse(c.Value)).ToList();
        Debug.Assert(times.Count == distances.Count, "Number of times and distances does not match!");
        var races = times.Zip(distances, (time, distance) => (time, distance)).ToList();

        foreach (var (time, distance) in races) {
            var waysToBeatRecord = WaysToBeatRecord(time, distance);
            result1 *= waysToBeatRecord;
        }

        var time2 = long.Parse(allLines[0].Split(":")[1].Replace(" ", ""));
        var distance2 = long.Parse(allLines[1].Split(":")[1].Replace(" ", ""));
        
        result2 = WaysToBeatRecord(time2, distance2);
    }

    private static long WaysToBeatRecord(long time, long distance) {
        var waysToBeatRecord = 0L;
        for (int i = 0; i < time; i++) {
            // hold the button for i milliseconds
            var distanceTraveled = (time - i) * i;
            if (distanceTraveled > distance)
                waysToBeatRecord++;
        }

        return waysToBeatRecord;
    }
}
