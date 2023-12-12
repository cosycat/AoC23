using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d12y2023; 

public static class Day12 {
    private const long ExpectedResultTest1 = 21;
    private const long ExpectedResultTest2 = 525152;
    private const string InputFileName = "inputDay12.txt";
    private const string TestFileName = "testInputDay12.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 7350; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 200097286528151; // For ensuring it stays correct, once the actual result is known
    
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
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0; 
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");

        // Process input line by line with regex
        const string mainPattern = $@"(?'line'\S+)\s(?:(?'number'\d+),?)+\s?(?'expected'\d+)?";
        Console.WriteLine($"Regex: {mainPattern}");
        for (int i = 0; i < allLines.Count; i++) {
            var totalLine = allLines[i];
            if (totalLine.Length == 0) break;
            var mainMatch = Regex.Match(totalLine, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == totalLine.Trim(), $"Line {i} does not match {mainMatch.Value}");
            var lineValues = LineFromString((mainMatch.Groups["line"].Value + ""));
            var numbers = mainMatch.Groups["number"].Captures.Select(c => long.Parse(c.Value)).ToList();

            // Part 1
            Cache.Clear();
            var arrangementsWithCache = CountArrangementsRecursive(lineValues, numbers, printSolution: false, useCache: true);
            result1 += arrangementsWithCache;

            // Part 2
            
            // Prepare line
            var line5 = mainMatch.Groups["line"].Value;
            var numbers5 = new List<long>();
            numbers5.AddRange(numbers);
            for (int j = 0; j < 4; j++) {
                line5 += "?" + mainMatch.Groups["line"].Value;
                numbers5.AddRange(numbers);
            }

            // Calculate line
            Console.Write($"Line {i}: {line5}, numbers: {string.Join(",", numbers5)}");
            var lineSprings5 = LineFromString(line5);
            Cache.Clear();
            var res5 = CountArrangementsRecursive(lineSprings5, numbers5, printSolution: false, useCache: true);
            result2 += res5;
            Console.WriteLine($" -> {res5}");
        }
        
    }

    
    private static string LineToString(SpringState[] lineSprings) {
        return string.Join("", lineSprings.Select(s => s switch {
            SpringState.Dot => '.',
            SpringState.Hashtag => '#',
            SpringState.Unknown => '?',
            _ => throw new ArgumentOutOfRangeException()
        }));
    }
    
    private static SpringState[] LineFromString(string line) {
        return line.ToCharArray().Select(c =>
            c switch {
                '.' => SpringState.Dot,
                '#' => SpringState.Hashtag,
                '?' => SpringState.Unknown,
                _ => throw new Exception($"Unknown spring state: {c}")
            }).ToArray();
    }
    
    private static readonly Dictionary<(long index, int numbersLeft), long> Cache = new();

    private static long CountArrangementsRecursive(SpringState[] lineSprings, List<long> numbers, long currIndex = 0, bool printSolution = false, bool useCache = true) {
        if (useCache && Cache.TryGetValue((currIndex, numbers.Count), out long value)) {
            if (printSolution) Console.WriteLine($"    Found in cache: ({currIndex},{numbers.Count}) -> {value}");

            return value;
        }
        
        if (printSolution) {
            Console.WriteLine($"    {LineToString(lineSprings)}");
            Console.WriteLine($"    {"".PadLeft((int) currIndex)}^");
        }

        if (currIndex >= lineSprings.Length) {
            Debug.Assert(currIndex == lineSprings.Length, $"currIndex ({currIndex}) is not at the end of the line ({lineSprings.Length}) numbers: {string.Join(",", numbers)}, line: {LineToString(lineSprings)}");
            if (numbers.Count > 0) return 0;
            if (printSolution) Console.WriteLine($"    Found one solution: {LineToString(lineSprings)}");
            return 1;
        }
        
        if (numbers.Count > 0 && currIndex + numbers.Sum() + numbers.Count - 1 > lineSprings.Length) {
            if (printSolution) Console.WriteLine($"    Not enough space left for all numbers: {string.Join(",", numbers)}");
            return 0;
        }

        if (numbers.Count > 0 && lineSprings.All(s => s == SpringState.Dot)) {
            if (printSolution) Console.WriteLine($"    All springs are operational, but we still have numbers left: {string.Join(",", numbers)}");
            return 0;
        }

        if (numbers.Count == 0 && lineSprings.Skip((int)currIndex).Any(s => s == SpringState.Hashtag)) {
            if (printSolution) Console.WriteLine($"    No numbers left, but we still have operational springs: {string.Join(",", numbers)}");
            return 0;
        }

        var currSpring = lineSprings[currIndex];
        switch (currSpring) {
            case SpringState.Dot:
                return CountArrangementsRecursive(lineSprings, numbers, currIndex + 1, printSolution);
            case SpringState.Hashtag:
                // check if possible
                if (numbers.Count == 0) return 0;
                var nextNumber = numbers[0];
                var nextIndex = currIndex + nextNumber;
                if (currIndex + nextNumber > lineSprings.Length) return 0;
                var prevStates = new SpringState[nextNumber + 1];
                var foundAContradiction = false;
                for (int i = 0; i < nextNumber; i++) {
                    if (lineSprings[currIndex + i] == SpringState.Dot) foundAContradiction = true;
                    // store state
                    prevStates[i] = lineSprings[currIndex + i];
                    // update state in case of unknown
                    lineSprings[currIndex + i] = SpringState.Hashtag;
                }

                // if we are not at the end of the line, check if the next spring is operational
                if (nextIndex < lineSprings.Length) {
                    prevStates[nextNumber] = lineSprings[currIndex + nextNumber];
                    if (prevStates[nextNumber] == SpringState.Hashtag) foundAContradiction = true;
                    lineSprings[currIndex + nextNumber] = SpringState.Dot;
                    nextIndex++;
                }

                
                // numbers.RemoveAt(0); // remove the number we just used
                // count arrangements
                var resD = foundAContradiction ? 0 : CountArrangementsRecursive(lineSprings, numbers.Skip(1).ToList(), nextIndex, printSolution);
                // restore state
                // numbers.Insert(0, nextNumber);
                for (int i = 0; i < prevStates.Length - 1; i++) {
                    lineSprings[currIndex + i] = prevStates[i];
                }
                if (currIndex + nextNumber < lineSprings.Length) {
                    lineSprings[currIndex + nextNumber] = prevStates[nextNumber];
                }

                return resD;
            case SpringState.Unknown:
                var resU = 0L;
                lineSprings[currIndex] = SpringState.Hashtag;
                resU += CountArrangementsRecursive(lineSprings, numbers, currIndex, printSolution);
                lineSprings[currIndex] = SpringState.Dot;
                resU += CountArrangementsRecursive(lineSprings, numbers, currIndex, printSolution);
                lineSprings[currIndex] = SpringState.Unknown;
                return ReturnAndStore(resU);
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        long ReturnAndStore(long result) {
            Debug.Assert(!Cache.ContainsKey((currIndex, numbers.Count)) || Cache[(currIndex, numbers.Count)] == result, $"Cache already contains key ({currIndex}, {numbers.Count}) with value {Cache[(currIndex, numbers.Count)]} but we want to store {result}");
            Cache[(currIndex, numbers.Count)] = result;
            if (printSolution)
                Console.WriteLine($"    Storing in cache: ({currIndex},{numbers.Count}) -> {result}");
            return result;
        }
    }
}

internal enum SpringState {
    Dot,
    Hashtag,
    Unknown
}
