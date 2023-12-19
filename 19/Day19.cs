using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d19y2023; 

public static class Day19 {
    private const long ExpectedResultTest1 = 19114;
    private const long ExpectedResultTest2 = 0; // TODO replace
    private const string InputFileName = "inputDay19.txt";
    private const string TestFileName = "testInputDay19.txt";
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
        
        var bothParts = File.ReadAllText(inputFileName).Split("\n\n").Select(part => part.Split('\n')).ToList();
        var instructions = bothParts.First();
        var input = bothParts.Last();

        const string instructionPattern = @"^(?'name'\w+)\{(?'rule'(?'part'[xmas])(?'than'[<>])(?'limit'\d+)\:(?'nextRule'\w+)\,)*(?'finalRule'\w+)\}$";
        Console.WriteLine($"Instruction Regex: {instructionPattern}");
        
        const string inputPattern = @"^\{x\=(?'x'\d+),m\=(?'m'\d+),a\=(?'a'\d+),s\=(?'s'\d+)\}$";
        Console.WriteLine($"Input Regex: {inputPattern}");
        
        var allInstructions = new Dictionary<string, Instruction>();
        
        // instructions
        for (int i = 0; i < instructions.Length; i++) {
            var line = instructions[i];
            var mainMatch = Regex.Match(line, instructionPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} ({line}) does not match {mainMatch.Value}");
            
            var name = mainMatch.Groups["name"].Value;
            var finalRuleName = mainMatch.Groups["finalRule"].Value;
            var rules = new List<Rule>();
            for (int j = 0; j < mainMatch.Groups["rule"].Captures.Count; j++) {
                var part = mainMatch.Groups["part"].Captures[j].Value;
                var smallerThan = mainMatch.Groups["than"].Captures[j].Value == "<";
                var limit = int.Parse(mainMatch.Groups["limit"].Captures[j].Value);
                var nextRuleName = mainMatch.Groups["nextRule"].Captures[j].Value;
                rules.Add(new Rule(Enum.Parse<RulePart>(part), smallerThan, limit, nextRuleName));
            }
            
            allInstructions[name] = new Instruction(name, rules, finalRuleName);
        }
        
        var allInputs = new List<Input>();
        
        // input
        for (int i = 0; i < input.Length; i++) {
            var line = input[i];
            var mainMatch = Regex.Match(line, inputPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} ({line}) does not match {mainMatch.Value}");
            
            var x = int.Parse(mainMatch.Groups["x"].Value);
            var m = int.Parse(mainMatch.Groups["m"].Value);
            var a = int.Parse(mainMatch.Groups["a"].Value);
            var s = int.Parse(mainMatch.Groups["s"].Value);
            
            allInputs.Add(new Input(x, m, a, s));
        }

        foreach (var inputLine in allInputs) {
            var nextRuleName = "in";
            while (nextRuleName is not "A" and not "R") {
                var rule = allInstructions[nextRuleName];
                nextRuleName = rule.GetNextRule(inputLine);
            }

            if (nextRuleName is "A") {
                result1 += inputLine.total;
            }
        }
        
    }
    
}

public class Input {
    public int X { get; }
    public int M { get; }
    public int A { get; }
    public int S { get; }
    
    public int total { get; }
    
    public Input(int x, int m, int a, int s) {
        X = x;
        M = m;
        A = a;
        S = s;
        total = x + m + a + s;
    }
}

public class Instruction {
    public string Name { get; }
    public List<Rule> Rules { get; }
    public string FinalRuleName { get; }
    
    public Instruction(string name, List<Rule> rules, string finalRuleName) {
        Name = name;
        Rules = rules;
        FinalRuleName = finalRuleName;
    }

    public string GetNextRule(Input input) {
        foreach (var rule in Rules) {
            var value = rule.Part switch {
                RulePart.x => input.X,
                RulePart.m => input.M,
                RulePart.a => input.A,
                RulePart.s => input.S,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (rule.SmallerThan && value < rule.Limit || !rule.SmallerThan && value > rule.Limit) {
                return rule.NextRuleName;
            }
        }

        return FinalRuleName;
    }
}

public class Rule {
    public RulePart Part { get; }
    public bool SmallerThan { get; }
    public int Limit { get; }
    public string NextRuleName { get; }
    
    public Rule(RulePart part, bool smallerThan, int limit, string nextRuleName) {
        Part = part;
        SmallerThan = smallerThan;
        Limit = limit;
        NextRuleName = nextRuleName;
    }
}

public enum RulePart {
    x,
    m,
    a,
    s
}
