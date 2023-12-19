using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d19y2023; 

public static class Day19 {
    private const long ExpectedResultTest1 = 19114;
    private const long ExpectedResultTest2 = 167409079868000L;
    private const string InputFileName = "inputDay19.txt";
    private const string TestFileName = "testInputDay19.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 330820; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 123972546935551; // For ensuring it stays correct, once the actual result is known
    
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
        var instructionLines = bothParts.First();
        var input = bothParts.Last();

        const string instructionPattern = @"^(?'name'\w+)\{(?'rule'(?'part'[xmas])(?'than'[<>])(?'limit'\d+)\:(?'nextRule'\w+)\,)*(?'finalRule'\w+)\}$";
        Console.WriteLine($"Instruction Regex: {instructionPattern}");
        
        const string inputPattern = @"^\{x\=(?'x'\d+),m\=(?'m'\d+),a\=(?'a'\d+),s\=(?'s'\d+)\}$";
        Console.WriteLine($"Input Regex: {inputPattern}");
        
        var allInstructions = new Dictionary<string, Instruction>();
        // instruction parsing
        for (int i = 0; i < instructionLines.Length; i++) {
            var line = instructionLines[i];
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

        // fill direct references in instructions and rules
        foreach (var instructionPair in allInstructions) {
            Debug.Assert(instructionPair.Key == instructionPair.Value.Name, "instructionPair.Key == instructionPair.Value.Name");
            var instruction = instructionPair.Value;
            foreach (var rule in instruction.Rules) {
                if (rule.NextInstructionName is "A" or "R") continue;
                rule.NextInstruction = allInstructions[rule.NextInstructionName];
            }
            if (instruction.FinalRuleName is not "A" and not "R") {
                instruction.FinalInstruction = allInstructions[instruction.FinalRuleName];
            }
        }
        
        var allInputs = new List<Input>();
        // input parsing
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

        var startInstruction = allInstructions["in"];
        var acceptingRanges = new List<InputRange>();
        var rejectingRanges = new List<InputRange>();
        var undecidedRange = new InputRange();
        
        startInstruction.CalculateForAllRules(acceptingRanges, rejectingRanges, undecidedRange);

        // Console.WriteLine($"Accepting ranges: ({acceptingRanges.Count}) {string.Join("; ", acceptingRanges)}");
        // Console.WriteLine($"Rejecting ranges: ({rejectingRanges.Count}) {string.Join("; ", rejectingRanges)}");
        
        result2 = acceptingRanges.Sum(range => range.GetTotal());
        
    }
    
}

public class InputRange {
    public const int LowerBound = 1;
    public const int UpperBound = 4000;
    
    public Range X { get; private set; }
    public Range M { get; private set; }
    public Range A { get; private set; }
    public Range S { get; private set; }

    public bool IsAllEmpty => IsEmptyRangeX && IsEmptyRangeM && IsEmptyRangeA && IsEmptyRangeS;
    private bool IsEmptyRangeX => X.Start.Value == X.End.Value && X.Start.Value < LowerBound || X.Start.Value > UpperBound;
    private bool IsEmptyRangeM => M.Start.Value == M.End.Value && M.Start.Value < LowerBound || M.Start.Value > UpperBound;
    private bool IsEmptyRangeA => A.Start.Value == A.End.Value && A.Start.Value < LowerBound || A.Start.Value > UpperBound;
    private bool IsEmptyRangeS => S.Start.Value == S.End.Value && S.Start.Value < LowerBound || S.Start.Value > UpperBound;

    public InputRange(Range x, Range m, Range a, Range s) {
        X = x;
        M = m;
        A = a;
        S = s;
    }

    public InputRange() {
        X = new Range(LowerBound - 1, UpperBound + 1);
        M = new Range(LowerBound - 1, UpperBound + 1);
        A = new Range(LowerBound - 1, UpperBound + 1);
        S = new Range(LowerBound - 1, UpperBound + 1);
    }

    public void SetRange(RulePart rulePart, Range newRange) {
        switch (rulePart) {
            case RulePart.x:
                X = newRange;
                break;
            case RulePart.m:
                M = newRange;
                break;
            case RulePart.a:
                A = newRange;
                break;
            case RulePart.s:
                S = newRange;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rulePart), rulePart, null);
        }
    }
    
    public long GetTotal() {
        var allRanges = new List<Range> {X, M, A, S};
        var total = 1L;
        foreach (var range in allRanges) {
            total *= Math.Min(range.End.Value, UpperBound) - Math.Max(range.Start.Value, LowerBound) + 1;
        }
        return total;
    }
    
    public override string ToString() {
        return $"X: {X}, M: {M}, A: {A}, S: {S}";
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
    public Instruction? FinalInstruction { get; set; } // TODO set
    
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
                return rule.NextInstructionName;
            }
        }

        return FinalRuleName;
    }
    
    private static int _indent = 0;
    private static string Indent => new(' ', _indent * 4);
    private const bool UseDebug = false;

    public void CalculateForAllRules(List<InputRange> acceptingRanges, List<InputRange> rejectingRanges, InputRange undecidedRange) {
        if (UseDebug) Console.WriteLine($"{Indent}➡️Calculating for {Name} with undecided range {undecidedRange}");
        
        foreach (var rule in Rules) {
            if (undecidedRange.IsAllEmpty) {
                if (UseDebug) Console.WriteLine($"{Indent}<-Undecided range {undecidedRange} is empty, returning");
                return;
            }

            if (UseDebug) Console.WriteLine($"{Indent}  Calculating for rule {rule.Part} {(rule.SmallerThan ? "<" : ">")} {rule.Limit} : {rule.NextInstructionName}");

            var relevantRange = rule.Part switch {
                RulePart.x => undecidedRange.X,
                RulePart.m => undecidedRange.M,
                RulePart.a => undecidedRange.A,
                RulePart.s => undecidedRange.S,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (rule.Limit <= relevantRange.Start.Value && rule.SmallerThan || rule.Limit >= relevantRange.End.Value && !rule.SmallerThan) { // TODO < or <= ?
                // rule is irrelevant
                if (UseDebug) Console.WriteLine($"{Indent}  <-Rule is irrelevant");
                continue;
            }
            var lowerRange = new Range(relevantRange.Start.Value, rule.SmallerThan ? rule.Limit - 1 : rule.Limit);
            var upperRange = new Range(rule.SmallerThan ? rule.Limit : rule.Limit + 1, relevantRange.End.Value);
            var lowerInputRange = new InputRange(undecidedRange.X, undecidedRange.M, undecidedRange.A, undecidedRange.S);
            lowerInputRange.SetRange(rule.Part, lowerRange);
            var upperInputRange = new InputRange(undecidedRange.X, undecidedRange.M, undecidedRange.A, undecidedRange.S);
            upperInputRange.SetRange(rule.Part, upperRange);
            
            undecidedRange = rule.SmallerThan ? upperInputRange : lowerInputRange;
            var ruleRelevantRange = rule.SmallerThan ? lowerInputRange : upperInputRange;
            
            if (rule.NextInstructionName is "A") {
                acceptingRanges.Add(ruleRelevantRange);
                if (UseDebug) Console.WriteLine($"{Indent}  <-Rule A: Returning for {ruleRelevantRange}");
                continue;
            }
            if (rule.NextInstructionName is "R") {
                rejectingRanges.Add(ruleRelevantRange);
                if (UseDebug) Console.WriteLine($"{Indent}  <-Rule R: Returning for {ruleRelevantRange}");
                continue;
            }

            Debug.Assert(rule.NextInstruction is not null, "rule.NextRule is not null");
            _indent++;
            rule.NextInstruction!.CalculateForAllRules(acceptingRanges, rejectingRanges, ruleRelevantRange);
            _indent--;

        }

        if (undecidedRange.IsAllEmpty) {
            if (UseDebug) Console.WriteLine($"{Indent}<-Undecided range {undecidedRange} is empty, returning");
            return;
        }

        if (FinalRuleName is "A") {
            acceptingRanges.Add(undecidedRange);
            if (UseDebug) Console.WriteLine($"{Indent}<-<-Final Rule A: Returning");
            return;
        }

        if (FinalRuleName is "R") {
            rejectingRanges.Add(undecidedRange);
            if (UseDebug) Console.WriteLine($"{Indent}<-<-Final Rule R: Returning");
            return;
        }

        Debug.Assert(FinalInstruction is not null, "rule.NextRule is not null");
        _indent++;
        FinalInstruction!.CalculateForAllRules(acceptingRanges, rejectingRanges, undecidedRange);
        _indent--;

        if (UseDebug) Console.WriteLine($"{Indent}<-Returning");
        
    }
}

public class Rule {
    public RulePart Part { get; }
    public bool SmallerThan { get; }
    public int Limit { get; }
    public string NextInstructionName { get; }
    public Instruction? NextInstruction { get; set; }

    public Rule(RulePart part, bool smallerThan, int limit, string nextInstructionName) {
        Part = part;
        SmallerThan = smallerThan;
        Limit = limit;
        NextInstructionName = nextInstructionName;
    }
}

public enum RulePart {
    x,
    m,
    a,
    s
}
