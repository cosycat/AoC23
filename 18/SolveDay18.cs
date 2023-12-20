using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d18y2023;

public static partial class Day18 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay18_00.txt", null, null) // TODO replace
    };

    private const /*resultType*/ int ActualResult1 = 0; // TODO replace
    private const /*resultType*/ int ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var width = allLines[0].Length;
        var height = allLines.Count;

        // Process input char by char
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                // TODO your code here..
            }
        }

        // Process input line by line with regex
        const string singleInputName = "SingleInput";
        const string singleInputPattern = @"\d+";
        const string mainPattern = $@"InputLine \d+:(?:\s*(?'{singleInputName}'{singleInputPattern}),?)+";
        // Regex for strings like "InputLine 1: 10,  2, 33,  4, 56, 78,  9"
        Console.WriteLine($"Regex: {mainPattern}");
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(),
                $"Line {i} does not match {mainMatch.Value}");
            var inputs = mainMatch.Groups[singleInputName].Captures.Select(c => long.Parse(c.Value)).ToList();

            // TODO your code here..
        }
    }
}