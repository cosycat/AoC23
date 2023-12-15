using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace d15y2023; 

public static class Day15 {
    private const long ExpectedResultTest1 = 1320;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay15.txt";
    private const string TestFileName = "testInputDay15.txt";
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

        foreach (var line in allLines) {
            var result = ProcessLine(line);
            // Console.WriteLine($"res: {result}");
        }
        
        result1 = ProcessLine(allLines[0]);
    }

    private static int ProcessLine(string line) {
        var input = line.Split(',');
        Debug.Assert(input.Length > 0, $"Input line {line} is empty!");

        var total = 0;
        foreach (var text in input) {
            var currVal = 0;
            foreach (var asciiVal in Encoding.ASCII.GetBytes(text)) {
                currVal += asciiVal;
                currVal *= 17;
                currVal %= 256;
            }
            total += currVal;
        }

        return total;
    }
}
