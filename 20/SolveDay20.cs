using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d20y2023;

public static partial class Day20 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay20.txt", 32000000, null),
        ("testInputDay20_2.txt", 11687500, null),
    };

    private const /*resultType*/ int ActualResult1 = 0; // TODO replace
    private const /*resultType*/ int ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        
        const string name = @"((?<broadcaster>broadcaster)|(?'symbol'[%&])(?'name'\w+))";
        const string mainPattern = $@"^{name}\s\-\>\s((?'destinations'\w+)(?:\,\s)?)+$";
        Console.WriteLine($"Regex: {mainPattern}");

        var broadcastInstructionNames = new List<string>();
        var allInstructions = new Dictionary<string, Instruction>();
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} does not match {mainMatch.Value}");
            
            var outgoingInstructionNames = mainMatch.Groups["destinations"].Captures.Select(c => c.Value).ToList();
            
            if (mainMatch.Groups["broadcaster"].Success) {
                broadcastInstructionNames.AddRange(outgoingInstructionNames);
                continue;
            }

            var instructionType = mainMatch.Groups["symbol"].Value switch {
                "%" => InstructionType.FlipFlop,
                "&" => InstructionType.Conjunction,
                _ => throw new ArgumentOutOfRangeException("", $"line {i} wrong instruction type: {line}")
            };
            var instructionName = mainMatch.Groups["name"].Value;

            var instruction = new Instruction(instructionType, outgoingInstructionNames);
            
            allInstructions.Add(instructionName, instruction);
        }

        foreach (var instruction in allInstructions.Values) {
            instruction.Init(allInstructions);
        }

        var repetitions = 4;
        var highPulseCount = 0L;
        var lowPulseCount = 0L;
        for (int i = 0; i < repetitions; i++) {
            Instruction.ResetCount();
            foreach (var broadcastInstructionName in broadcastInstructionNames) {
                allInstructions[broadcastInstructionName].DoInstruction(Pulse.Low, new Instruction(InstructionType.Broadcast, new List<string>(0)));
            }
            highPulseCount += Instruction.HighPulseCount;
            lowPulseCount += Instruction.LowPulseCount;
            Console.WriteLine($"{i} HighPulses: {Instruction.HighPulseCount}, LowPulses: {Instruction.LowPulseCount}");
        }
        
        // Console.WriteLine($"HighPulses: {highPulseCount}, LowPulses: {lowPulseCount}");
        result1 = highPulseCount * lowPulseCount;
    }
}

public class Instruction {
    public Instruction(InstructionType instructionType, List<string> outgoingInstructionNames) {
        OutgoingInstructionNames = outgoingInstructionNames;
        InstructionType = instructionType;
        switch (instructionType) {
            case InstructionType.FlipFlop:
                DoInstruction = DoInstructionFlipFlop;
                break;
            case InstructionType.Conjunction:
                DoInstruction = DoInstructionConjunction;
                break;
            case InstructionType.Sink:
                DoInstruction = DoInstructionSink;
                break;
            case InstructionType.Broadcast:
                DoInstruction = (_, _) => throw new Exception();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private List<Instruction> OutgoingInstructions { get; } = new();
    private List<string> OutgoingInstructionNames { get; }
    
    // Conjunction Only
    private Dictionary<Instruction, Pulse> IncomingInstructions { get; } = new();
    // FlipFlop Only
    private bool State { get; set; } = false;
    
    private InstructionType InstructionType { get; }

    public readonly Action<Pulse, Instruction> DoInstruction;

    public static long LowPulseCount { get; private set; } = 1; // one from the button
    public static long HighPulseCount { get; private set; } = 0;

    private void DoInstructionFlipFlop(Pulse pulse, Instruction sender) {
        CountPulse(pulse);
        
        if (pulse == Pulse.High) {
            // Console.WriteLine("<-End");
            return;
        }

        State = !State;
        foreach (var outgoingInstruction in OutgoingInstructions) {
            outgoingInstruction.DoInstruction(State ? Pulse.High : Pulse.Low, this);
        }
    }

    private void DoInstructionConjunction(Pulse pulse, Instruction sender) {
        CountPulse(pulse);

        Debug.Assert(IncomingInstructions.ContainsKey(sender), $"Sender missing!");
        IncomingInstructions[sender] = pulse;

        var pulseToSend = Pulse.Low;
        foreach (var value in IncomingInstructions.Values) {
            if (value != Pulse.Low) continue;
            pulseToSend = Pulse.High;
            break;
        }

        foreach (var outgoingInstruction in OutgoingInstructions) {
            outgoingInstruction.DoInstruction(pulseToSend, this);
        }
        
    }

    private void DoInstructionSink(Pulse pulse, Instruction sender) {
        CountPulse(pulse);
        // Console.WriteLine("<-End");
    }

    private static void CountPulse(Pulse pulse) {
        // Console.WriteLine($"Count Pulse {pulse}");
        switch (pulse) {
            case Pulse.High:
                HighPulseCount++;
                break;
            case Pulse.Low:
                LowPulseCount++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pulse), pulse, null);
        }
    }

    public void Init(Dictionary<string,Instruction> allInstructions) {
        foreach (var outgoingInstructionName in OutgoingInstructionNames) {
            if (!allInstructions.TryGetValue(outgoingInstructionName, out var outgoingInstruction)) {
                outgoingInstruction = new Instruction(InstructionType.Sink, new List<string>(0));
            }
            OutgoingInstructions.Add(outgoingInstruction);
            outgoingInstruction.IncomingInstructions[this] = Pulse.Low;
        }
    }

    public static void ResetCount() {
        HighPulseCount = 0;
        LowPulseCount = 1; // one from the button
    }
}

public enum InstructionType {
    FlipFlop,
    Conjunction,
    Broadcast,
    Sink
}

public enum Pulse {
    High,
    Low
}
