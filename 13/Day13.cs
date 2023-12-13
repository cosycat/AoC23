using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d13y2023; 

public static class Day13 {
    private const long ExpectedResultTest1 = 405;
    private const long ExpectedResultTest2 = 400;
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
            var linesHor = new Fields[height][];
            var linesVert = new Fields[width][];
            //init
            for (int y = 0; y < height; y++) {
                linesHor[y] = new Fields[width];
                for (int x = 0; x < width; x++) {
                    var c = lines[y][x];
                    linesHor[y][x] = c switch {
                        '.' => Fields.Ash,
                        '#' => Fields.Rock,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            for (int x = 0; x < width; x++) {
                linesVert[x] = new Fields[height];
                for (int y = 0; y < height; y++) {
                    var c = lines[y][x];
                    linesVert[x][y] = c switch {
                        '.' => Fields.Ash,
                        '#' => Fields.Rock,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }


            var isOriginalHorizontalLine = false;
            var possibleMatches = CheckFieldForSymmetry(linesHor);
            if (possibleMatches.Count == 0) {
                possibleMatches = CheckFieldForSymmetry(linesVert);
                isOriginalHorizontalLine = true;
            }
            Debug.Assert(possibleMatches.Count == 1, $"Not exactly 1 possible match: {string.Join(", ", possibleMatches)}");

            var originalSymmetryLine = possibleMatches[0];
            Console.WriteLine($"Found Split: {originalSymmetryLine} (line is {(isOriginalHorizontalLine ? "horizontal" : "vertical")})");
            
            result1 += isOriginalHorizontalLine ? originalSymmetryLine * 100 : originalSymmetryLine;

            var foundLine = false;
            for (int y = 0; y < height && !foundLine; y++) {
                for (int x = 0; x < width && !foundLine; x++) {
                    var c = linesHor[y][x];
                    Debug.Assert(c == linesVert[x][y], $"Fields don't match at {x}, {y}");
                    linesHor[y][x] = c switch {
                        Fields.Ash => Fields.Rock,
                        Fields.Rock => Fields.Ash,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    linesVert[x][y] = linesHor[y][x];

                    var isNewHorizontalLine = false;
                    var possibleNewMatches = CheckFieldForSymmetry(linesHor);
                    if (!isOriginalHorizontalLine && possibleNewMatches.Contains(originalSymmetryLine)) {
                        possibleNewMatches.Remove(originalSymmetryLine);
                    }
                    if (possibleNewMatches.Count != 1) {
                        possibleNewMatches = CheckFieldForSymmetry(linesVert);
                        if (isOriginalHorizontalLine && possibleNewMatches.Contains(originalSymmetryLine)) {
                            possibleNewMatches.Remove(originalSymmetryLine);
                        }
                        isNewHorizontalLine = true;
                    }
                    
                    if (possibleNewMatches.Count == 1) {
                        var newSymmetryLine = possibleNewMatches[0];
                        Console.WriteLine($"Found New Line: {newSymmetryLine} at {x}, {y} (vert: {isNewHorizontalLine})");
                        result2 += isNewHorizontalLine ? newSymmetryLine * 100 : newSymmetryLine;
                        foundLine = true;
                    }
                    if (possibleNewMatches.Count > 1) {
                        Console.WriteLine($"Found multiple possible matches: {string.Join(", ", possibleNewMatches)}");
                    }
                    linesHor[y][x] = c;
                    linesVert[x][y] = c;
                }
            }
            Debug.Assert(foundLine, "No new symmetry line found!");
            
        }
        
    }

    private static List<int> CheckFieldForSymmetry(Fields[][] linesHor) {
        var firstLine = linesHor[0];
        var possibleMatches = new List<int>();
        for (int x = 0; x < firstLine.Length; x++) {
            if (IsPositionSymmetric(line: firstLine, position: x).isSymetric) {
                possibleMatches.Add(x);
            }
        }

        for (int y = 1; y < linesHor.Length; y++) {
            var line = linesHor[y];
            for (int i = 0; i < possibleMatches.Count; i++) {
                if (!IsPositionSymmetric(line, possibleMatches[i]).isSymetric) {
                    possibleMatches.RemoveAt(i);
                    i--;
                }
            }
        }

        return possibleMatches;
    }

    private static (bool isSymetric, int count) IsPositionSymmetric(Fields[] line, int position) {
        if (position <= 0) return (false, -1);
        if (position >= line.Length) return (false, -1);
        int steps = 0;
        while (position - steps - 1 >= 0 && position + steps < line.Length) {
            if (line[position - steps - 1] != line[position + steps]) {
                return (false, -1);
            }
            steps++;
        }
        
        return (true, steps);
    }
    
}

enum Fields {
    Rock,
    Ash
}
