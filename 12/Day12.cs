using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d12y2023; 

public static class Day12 {
    private const long ExpectedResultTest1 = 21 + 1 + 2 + 3 + 2 + 1 + 1 + 1 + 1 + 1 + 1;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay12.txt";
    private const string TestFileName = "testInputDay12.txt";
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
            // if (line.Length == 0) break;
            // Console.WriteLine($"Line {i}: {line}");
            var mainMatch = Regex.Match(totalLine, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == totalLine.Trim(), $"Line {i} does not match {mainMatch.Value}");
            currLine = mainMatch.Groups["line"].Value + ".....";
            var lineSprings = LineFromString((mainMatch.Groups["line"].Value + ""));
            var numbers = mainMatch.Groups["number"].Captures.Select(c => long.Parse(c.Value)).ToList();
            
            currResult = 0;
            var countArrangements = CountArrangementsRecursive(lineSprings, numbers, printSolution: false);
            currResult = 0;
            var countArrangementsWithMoreDots = CountArrangementsRecursive(LineFromString(currLine), numbers, printSolution: false);
            var expected = mainMatch.Groups["expected"].Success ? long.Parse(mainMatch.Groups["expected"].Value) : -1;
            if (countArrangementsWithMoreDots != countArrangements ||
                (mainMatch.Groups["expected"].Success && (countArrangements != expected || currResult != expected || countArrangementsWithMoreDots != expected))) {
                Console.WriteLine($"Line {i} {totalLine} - expected {mainMatch.Groups["expected"].Value} (currResult: {currResult}, countArrangements: {countArrangements}, countArrangementsWithMoreDots: {countArrangementsWithMoreDots})");
                currResult = 0;
                // CountArrangementsRecursive(lineSprings, numbers, printSolution: true);
                Console.WriteLine($"Normal line:");
                CountArrangementsRecursive(lineSprings, numbers, printSolution: true);
                currResult = 0;
                Console.WriteLine($"With more dots:");
                CountArrangementsRecursive(LineFromString(currLine), numbers, printSolution: true);
            }
            result1 += countArrangements;
        }
        
    }

    private static long currResult = 0;
    private static string currLine;
    
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

    private static long CountArrangementsRecursive(SpringState[] lineSprings, List<long> numbers, long currIndex = 0, bool printSolution = false) {
        if (printSolution) {
            Console.WriteLine($"    {LineToString(lineSprings)}");
            Console.WriteLine($"    {"".PadLeft((int) currIndex)}^");
        }

        if (currIndex >= lineSprings.Length) {
            Debug.Assert(currIndex == lineSprings.Length, $"currIndex ({currIndex}) is not at the end of the line ({lineSprings.Length}) numbers: {string.Join(",", numbers)}, line: {LineToString(lineSprings)}");
            if (numbers.Count > 0) return 0;
            if (printSolution) {
                Console.WriteLine($"    Found one solution: {LineToString(lineSprings)}");
                Console.WriteLine($"                        {currLine}");
            }

            currResult++;
            return 1;
            // return IsValidLine(lineSprings, numbers) ? 1 : 0; // found one solution
        }
        
        if (numbers.Count > 0 && currIndex + numbers.Sum() + numbers.Count - 1 > lineSprings.Length) return 0; // not enough space left for all the numbers
        if (numbers.Count > 0 && lineSprings.All(s => s == SpringState.Dot)) return 0; // all springs are operational, but we still have numbers left
        
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
                return resU;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static bool IsValidLine(SpringState[] lineSprings, List<long> numbers) {
        Debug.Assert(!lineSprings.Contains(SpringState.Unknown), "Line contains unknown springs!");
        var currLineIndex = 0;
        var currNumberIndex = 0;
        while (currLineIndex < lineSprings.Length) {
            var currSpring = lineSprings[currLineIndex];
            switch (currSpring) {
                case SpringState.Dot:
                    currLineIndex++;
                    break;
                case SpringState.Hashtag:
                    if (currNumberIndex >= numbers.Count) return false;
                    if (currLineIndex + numbers[currNumberIndex] >= lineSprings.Length) return false;
                    for (int i = 0; i < numbers[currNumberIndex]; i++) {
                        if (lineSprings[currLineIndex + i] != SpringState.Hashtag) return false;
                    }
                    currLineIndex += (int) numbers[currNumberIndex];
                    if (currLineIndex < lineSprings.Length && lineSprings[currLineIndex] == SpringState.Hashtag) return false;
                    currLineIndex++;
                    currNumberIndex++;
                    break;
                case SpringState.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return true;
    }


    private static List<(int numbersUsed, int count, int partialNumberUsed)>?[] _possibilitiesPerIndex;

    private static List<(int numbersUsed, int count, int partialNumberUsed)> GetPossibilitiesFor(int index) {
        while (index >= 0 && _possibilitiesPerIndex[index] == null) index--;
        if (index < 0) return new List<(int numbersUsed, int count, int partialNumberUsed)>();
        return _possibilitiesPerIndex[index]!;
    }

    private static long SolveLine(SpringState[] lineSprings, List<long> numbers) {
        _possibilitiesPerIndex = new List<(int numbersUsed, int count, int partialNumberUsed)>[lineSprings.Length];
        
        var result = 0L;
        
        var lineIndex = 0;

        while (lineIndex < lineSprings.Length) {
            var currSpring = lineSprings[lineIndex];
            var possibilitiesBefore = GetPossibilitiesFor(lineIndex - 1);

            if (currSpring == SpringState.Dot) {
                while (possibilitiesBefore.Any(p => p.numbersUsed < numbers.Count)) {
                    possibilitiesBefore.RemoveAt(possibilitiesBefore.FindIndex(p => p.numbersUsed < numbers.Count));
                }
                lineIndex++;
                continue;
            }

            if (currSpring == SpringState.Hashtag) {
                // if (lineIndex == 0 || lineSprings[lineIndex-1] == SpringState.Operational)
                // RemoveAllLastNonPartialPossibilities(possibilitiesBefore);
                
            }
            
            var restOfSprings = lineSprings.Skip(lineIndex).ToArray();
            var numbersPossible = possibilitiesBefore.Where(p => p.count > 0);
            foreach (var possibleNumber in numbersPossible) {
                Debug.Assert(possibleNumber.numbersUsed >= 0, $"Can't use less than 0 numbers");
                Debug.Assert(possibleNumber.numbersUsed <= numbers.Count, $"Can't use more numbers than available");
                if (possibleNumber.numbersUsed == numbers.Count && restOfSprings.Contains(SpringState.Hashtag)) continue; // we have used all Numbers, but need more
                
            }
        }

        return result;

    }
    
}

internal enum SpringState {
    Dot,
    Hashtag,
    Unknown
}
