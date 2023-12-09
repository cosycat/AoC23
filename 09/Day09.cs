using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d09y2023; 

public static class Day09 {
    private const long ExpectedResultTest1 = 114;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay09.txt";
    private const string TestFileName = "testInputDay09.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 1772145754; // For ensuring it stays correct, once the actual result is known
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
        
        for (int i = 0; i < allLines.Count; i++) {
            var inputLine = allLines[i].Split(" ").Select(int.Parse);
            var lines = new List<List<int>> { inputLine.ToList() };
            while (!LastLineIsZeroes(lines)) {
                var newLine = new List<int>();
                var lastLine = lines[^1];
                for (int j = 0; j < lastLine.Count - 1; j++) {
                    newLine.Add(lastLine[j + 1] - lastLine[j]);
                }
                lines.Add(newLine);
            }

            for (int j = lines.Count - 2; j >= 0; j--) {
                var lastElementJ = lines[j][^1];
                var lastElementJ1 = lines[j + 1][^1];
                lines[j].Add(lastElementJ + lastElementJ1);
            }

            result1 += lines[0][^1];
        }
        
        
    }

    private static bool LastLineIsZeroes(List<List<int>> lines) {
        var lastLine = lines[^1];
        return lastLine.All(i => i == 0);
    }
}
