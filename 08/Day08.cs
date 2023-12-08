using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d08y2023; 

public static class Day08 {
    private const long ExpectedResultTest1 = 2;
    private const long ExpectedResultTest2 = 6;
    private const string InputFileName = "inputDay08.txt";
    private const string TestFileName = "testInputDay08.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 21389; // For ensuring it stays correct, once the actual result is known
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
        
        Dictionary<string, Node> nodes = new();

        const string singleInputPattern = @"(?'name'\w\w\w)";
        const string mainPattern = $@"{singleInputPattern} = \({singleInputPattern}, {singleInputPattern}\)";
        Console.WriteLine($"Regex: {mainPattern}");
        for (int i = 2; i < allLines.Count; i++) {
            var line = allLines[i];
            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} does not match {mainMatch.Value}");
            var names = mainMatch.Groups["name"].Captures.ToList();
            Debug.Assert(names.Count == 3, $"Line {i} does not have 3 names, but {names.Count}");
            var name = names[0].Value;
            var leftName = names[1].Value;
            var rightName = names[2].Value;
            Debug.Assert(!nodes.ContainsKey(name), $"Node {name} already exists!");
            nodes.Add(name, new Node(name, leftName, rightName));
        }

        foreach (var key in nodes.Keys) {
            nodes[key].Init(nodes);
        }
        
        var instructions = allLines[0].ToCharArray();
        var instructionsLength = instructions.Length;
        
        // PART 1
        if (nodes.ContainsKey("AAA") && nodes.ContainsKey("ZZZ")) {
            var (stepCount, endNode) = CountSteps(new List<Node> {nodes["AAA"]}, instructions, (node, _) => node.Name == "ZZZ");
            result1 = stepCount;
            Console.WriteLine($"Steps: {stepCount}, end node: {endNode.Name}");
        }
        else {
            Console.WriteLine("No AAA or ZZZ found!");
        }
        
        // PART 2
        var startNodes = new List<Node>();
        foreach (var key in nodes.Keys) {
            if (key.EndsWith("A")) startNodes.Add(nodes[key]);
        }
        Console.WriteLine($"Start nodes: {string.Join(", ", startNodes.Select(node => node.Name))}");
        Console.WriteLine($"End nodes: {string.Join(", ", nodes.Values.Where(node => node.Name.EndsWith("Z")).Select(node => node.Name))}");
        
        const bool useBruteForce = true;
        if (useBruteForce) {
            var (stepCount, _) = CountSteps(startNodes, instructions, (node, _) => node.Name.EndsWith("Z"));
            result2 = stepCount;
            return;
        }
        
        // var endNodes = nodes.Values.Where(node => node.Name.EndsWith("Z")).ToList();
        // var steps = startNodes.Select(startNode => { // Add all steps to all nodes before we loop back to the start node again
        //     var steps = new List<long>();
        //     foreach (var endNode in endNodes) {
        //         var (possibleSteps, foundNode) = CountSteps(new List<Node> {startNode}, instructions, (node, steps) => node == endNode || node == startNode && steps > 0);
        //         if (foundNode != startNode)
        //             steps.Add(possibleSteps);
        //     }
        //     return (startNode, steps);
        // }).ToList();
        //
        // foreach (var path in steps) {
        //     Console.WriteLine($"{path.startNode.Name}: {string.Join(", ", path.steps)}");
        // }
        // var currStepsCountPerStartNode = new List<long>();
        // for (var i = 0; i < startNodes.Count; i++) {
        //     currStepsCountPerStartNode.Add(steps[i].steps.Min());
        // }
        
    }

    private static (long stepCount, Node endNode) CountSteps(List<Node> startNodes, char[] instructions, Func<Node, long, bool> endChecker) {
        long steps = 0;
        var instructionIndex = 0;
        var currentNodes = new List<Node>();
        for (int i = 0; i < startNodes.Count; i++) {
            currentNodes.Add(startNodes[i]);
        }
        var loops = 0;
        while (currentNodes.Any(node => !endChecker(node, steps))) {
            var instruction = instructions[instructionIndex];
            currentNodes = instruction switch {
                'L' => currentNodes.Select(node => node.Left).ToList(),
                'R' => currentNodes.Select(node => node.Right).ToList(),
                _ => throw new Exception($"Unknown instruction {instruction}")
            };

            instructionIndex++;
            if (instructionIndex >= instructions.Length) {
                loops++;
                instructionIndex = 0;
            }
            steps++; // Count steps
            for (var i = 0; i < currentNodes.Count; i++) {
                var currentNode = currentNodes[i];
                if (currentNode.Name.EndsWith("A")) {
                    Console.WriteLine($"Looped for node {i}: {loops} times, steps: {steps}");
                }
            }

            // if (instructionIndex == 0) 
            //     Console.WriteLine($"Steps: {steps}, current nodes: {string.Join(", ", currentNodes.Select(node => node.Name))}");
        }

        return (steps, startNodes.First());
    }
}

public class Node {
    public string Name { get; }
    public Node Left { get; private set; }
    public Node Right { get; private set; }
    public string LeftName { get; }
    public string RightName { get; }
    
    public Node(string name, string leftName, string rightName) {
        Name = name;
        LeftName = leftName;
        RightName = rightName;
    }
    
    public void Init(Dictionary<string, Node> nodes) {
        Left = nodes[LeftName];
        Right = nodes[RightName];
    }
}
