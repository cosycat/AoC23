using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d05y2023; 

public static class Day05 {
    private const long ExpectedResultTest1 = 35;
    private const long ExpectedResultTest2 = 46;
    private const string InputFileName = "inputDay05.txt";
    private const string TestFileName = "testInputDay05.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const int ActualResult1 = 309796150;
    private const int ActualResult2 = 50716416;
    private const string Success = "✅";
    private const string Fail = "❌";

    public static void Main(string[] args) {
        TestRun();

        Stopwatch sw = new();
        sw.Start();
        Solve(InputFileName, out var result1, out var result2, printTimeRes1: () => Console.WriteLine($"Result 1 found in: {sw.ElapsedMilliseconds}ms"));
        sw.Stop();
        PrintResult(result1, ActualResult1, 1);
        PrintResult(result2, ActualResult2, 2);
        Console.WriteLine($"Total Time: {sw.ElapsedMilliseconds}ms");
    }
    
    private static void PrintResult(long result, long expected, int index, bool isTest = false) {
        Console.WriteLine($"{(isTest ? "Test " : "")}Result {index}: {result} {(expected == 0 ? "" : expected == result ? Success : Fail + $"(expected: {expected})")} ");
    }
    
    [Conditional("DEBUG")]
    private static void TestRun() {
        Solve(TestFileName, out var resultTest1, out var resultTest2, null);
        const string success = "✅";
        const string fail = "❌";
        Console.WriteLine($"Test result 1: {(resultTest1 == ExpectedResultTest1 ? success : fail)} (result: {resultTest1}, expected: {ExpectedResultTest1})");
        if (Test2Started)
            Console.WriteLine($"Test result 2: {(resultTest2 == ExpectedResultTest2 ? success : fail)} (result: {resultTest2}, expected: {ExpectedResultTest2})");
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, out long result2, Action? printTimeRes1) {
        result1 = 0; 
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        var wholeInput = string.Join("\n", allLines);
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");

        var seeds1 = Regex.Match(allLines[0], @"seeds: (?'Seed'\d+\s?)+").Groups["Seed"].Captures.Select(c => long.Parse(c.Value)).ToList();
        var destinations1 = new long[seeds1.Count];
        for (var i = 0; i < seeds1.Count; i++) {
            destinations1[i] = seeds1[i];
        }

        var match = Regex.Match(allLines[0], @"seeds: (?:(?'Seed'\d+)\s(?'Length'\d+\s))+");
        Debug.Assert(match.Success, "line does not match pattern");
        var seeds2 = match.Groups["Seed"].Captures.Select(c => long.Parse(c.Value)).ToList();
        var lengthsSeeds2 = match.Groups["Length"].Captures.Select(c => long.Parse(c.Value)).ToList();
        var seedAndLengths2 = seeds2.Zip(lengthsSeeds2, (seed, length) => (seed, length)).ToList();
        Debug.Assert(seeds2.Count == lengthsSeeds2.Count, "Seeds and lengths count mismatch");
        Console.WriteLine($"counting seeds {seeds2.Count} -> {seedAndLengths2.Aggregate(0L, (a, s) => a + s.length)}");
        
        var aToBs = new List<AtoB>();

        foreach (var aToBInput in wholeInput.Split("\n\n")) {
            if (aToBInput.StartsWith("seeds:")) continue;
            var aToB = new AtoB();
            foreach (var line in aToBInput.Split("\n")) {
                if (line.EndsWith("map:")) continue;
                var lineMatch = Regex.Match(line, @"(?'StartB'\d+)\s(?'StartA'\d+)\s(?'Length'\d+)");
                Debug.Assert(lineMatch.Success, $"Line {line} does not match pattern");
                var startB = long.Parse(lineMatch.Groups["StartB"].Value);
                var startA = long.Parse(lineMatch.Groups["StartA"].Value);
                var length = long.Parse(lineMatch.Groups["Length"].Value);
                aToB.AddRange(startB, startA, length);
            }
            aToB.SortRanges();
            aToBs.Add(aToB);
            for (int i = 0; i < destinations1.Length; i++) {
                destinations1[i] = aToB.GetB(destinations1[i]);
            }
        }

        result1 = destinations1.Min();
        printTimeRes1?.Invoke();

        foreach (var atoB in aToBs) {
            var seedsToProcess = new Stack<(long seed, long length)>();
            foreach (var s in seedAndLengths2) {
                seedsToProcess.Push(s);
            }
            seedAndLengths2.Clear();
            while (seedsToProcess.Count > 0) {
                var (a, length) = seedsToProcess.Pop();
                var b = atoB.GetB(a);
                var nextJump = atoB.GetNextJump(a, length);
                if (nextJump.HasValue) {
                    var (nextA, nextB) = nextJump.Value;
                    var prevLength = nextA - a;
                    var nextLength = length - prevLength;
                    seedAndLengths2.Add((b, prevLength));
                    seedsToProcess.Push((nextA, nextLength));
                } else {
                    seedAndLengths2.Add((b, length));
                }
            }
        }

        result2 = seedAndLengths2.Select(s => s.seed).Min();

    }
    
}

public class AtoB {
    
    private List<(long startA, long startB, long length)> Ranges { get; } = new();
    
    public void AddRange(long startB, long startA, long length) {
        Ranges.Add((startA, startB, length));
    }
    
    public void SortRanges() {
        Ranges.Sort((a, b) => a.startA.CompareTo(b.startA));
    }
    
    public long GetB(long a) {
        var nextLower = Ranges.FindLastIndex(r => r.startA <= a);
        if (nextLower == -1) return a;
        var range = Ranges[nextLower];
        if (range.startA + range.length <= a) return a;
        return range.startB + (a - range.startA);
    }

    public (long a, long b)? GetNextJump(long a, long length) {
        var nextLower = Ranges.FindLastIndex(r => r.startA <= a);
        var nextHigher = Ranges.FindIndex(r => r.startA > a);
        if (nextHigher == -1) return null;
        var nextHigherRange = Ranges[nextHigher];
        (long startA, long startB, long length) nextLowerRange = nextLower == -1 ? (0L, 0L, nextHigherRange.startA) : Ranges[nextLower];
        if (a + length <= nextLowerRange.startA + nextLowerRange.length) return null;
        if (a + length <= nextHigherRange.startA) return null;
        return (nextHigherRange.startA, nextHigherRange.startB);
    }
    
}
