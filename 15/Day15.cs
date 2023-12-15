using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace d15y2023; 

public static class Day15 {
    private const long ExpectedResultTest1 = 1320;
    private const long ExpectedResultTest2 = 145;
    private const string InputFileName = "inputDay15.txt";
    private const string TestFileName = "testInputDay15.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 497373; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 259356; // For ensuring it stays correct, once the actual result is known
    
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

        (result1, result2) = ProcessLine(allLines[0]);
    }

    private static (int totalHash, int focusingPower) ProcessLine(string line) {
        var input = line.Split(',');
        Debug.Assert(input.Length > 0, $"Input line {line} is empty!");

        var totalHash = 0;
        List<List< (string name, int value)>> hashmap = new();
        for (int i = 0; i < 256; i++) {
            hashmap.Add(new List<(string, int)>());
        }

        foreach (var text in input) {
            var (hash, labelHash, instruction) = ProcessSingleInput(text);
            totalHash += hash;
            Debug.Assert(hashmap[labelHash].Count(x => x.name == instruction.Label) <= 1, $"Hashmap[{labelHash}] contains more than one {instruction.Label}!");
            switch (instruction.Sign) {
                case "-":
                    hashmap[labelHash].RemoveAll(x => string.Equals(x.name, instruction.Label, StringComparison.Ordinal));
                    break;
                case "=":
                    var index = hashmap[labelHash].FindIndex(x =>
                        string.Equals(x.name, instruction.Label, StringComparison.Ordinal));
                    if (index >= 0)
                        hashmap[labelHash][index] = (instruction.Label, instruction.Number);
                    else
                        hashmap[labelHash].Add((instruction.Label, instruction.Number));
                    break;
            }

            // Console.WriteLine($"After \"{text}\":");
            // PrintHashmap(hashmap);
            // Console.WriteLine();
            
        }

        var focusingPower = 0;
        for (int boxNr = 0; boxNr < hashmap.Count; boxNr++) {
            var box = hashmap[boxNr];
            if (box.Count == 0)
                continue;
            for (int i = 0; i < box.Count; i++) {
                var lens = box[i];
                focusingPower += (boxNr + 1) * (i + 1) * lens.value;
            }
        }

        return (totalHash, focusingPower);
    }

    public static void PrintHashmap(List<List<(string name, int value)>> hashmap) {
        for (int boxNr = 0; boxNr < hashmap.Count; boxNr++) {
            var box = hashmap[boxNr];
            if (box.Count == 0)
                continue;
            Console.WriteLine($"Box {boxNr}: {string.Join(", ", box.Select(x => $"{x.name}({x.value})"))}");
        }
    }
    
    private static (int totalHash, int labelHash, Instruction) ProcessSingleInput(string text) {
        var totalHash = 0;
        var labelHash = 0;
        foreach (var asciiVal in Encoding.ASCII.GetBytes(text)) {
            totalHash += asciiVal;
            totalHash *= 17;
            totalHash %= 256;
        }
        
        var match = Regex.Match(text, @"(?'label'\w+)((?'sign'\-)|((?'sign'\=)(?'number'\d+)))");
        Debug.Assert(match.Success, $"Input {text} is not valid!");
        var label = match.Groups["label"].Value;
        var sign = match.Groups["sign"].Value;
        var number = match.Groups["number"].Value;

        foreach (var labelAscii in Encoding.ASCII.GetBytes(label)) {
            labelHash += labelAscii;
            labelHash *= 17;
            labelHash %= 256;
        }

        return (totalHash, labelHash, new Instruction(label, sign, number.Length == 0 ? -1 : int.Parse(number)));
    }
}

internal class Instruction {
    public string Label { get; }
    public string Sign { get; }
    public int Number { get; }
    public Instruction(string label, string sign, int number) {
        Label = label;
        Sign = sign;
        Number = number;
    }
}
