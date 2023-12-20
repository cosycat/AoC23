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

        var allInstructions = new Dictionary<string, Instruction>();
        Instruction? broadcastInstruction = null;
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            var mainMatch = Regex.Match(line, mainPattern);
            Debug.Assert(mainMatch.Success && mainMatch.Value.Trim() == line.Trim(), $"Line {i} does not match {mainMatch.Value}");
            
            var outgoingInstructionNames = mainMatch.Groups["destinations"].Captures.Select(c => c.Value).ToList();
            
            if (mainMatch.Groups["broadcaster"].Success) {
                broadcastInstruction = new Instruction(InstructionType.Broadcast, outgoingInstructionNames);
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
        Debug.Assert(broadcastInstruction != null);

        foreach (var instruction in allInstructions.Values) {
            instruction.Init(allInstructions);
        }
        broadcastInstruction.Init(allInstructions);

        var repetitions = 1000;
        var highPulseCount = 0L;
        var lowPulseCount = 0L;
        for (int i = 0; i < repetitions; i++) {
            Instruction.ResetCount();
            var nextPulses = new List<(Instruction next, Pulse pulse, Instruction sender)>();
            nextPulses.Add((broadcastInstruction, Pulse.Low, null));
            while (nextPulses.Count > 0) {
                var newPulses = new List<(Instruction next, Pulse pulse, Instruction sender)>();
                foreach (var (next, pulse, sender) in nextPulses) {
                    newPulses.AddRange(next.DoInstruction(pulse, sender));
                }
                nextPulses = newPulses;
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
                DoInstruction = DoInstructionBroadcast;
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

    public readonly Func<Pulse, Instruction, List<(Instruction next, Pulse pulse, Instruction sender)>> DoInstruction;

    public static long LowPulseCount { get; private set; } = 1; // one from the button
    public static long HighPulseCount { get; private set; } = 0;

    private List<(Instruction next, Pulse pulse, Instruction sender)> DoInstructionFlipFlop(Pulse pulse, Instruction sender) {
        CountPulse(pulse);
        
        if (pulse == Pulse.High) {
            // Console.WriteLine("<-End");
            return new List<(Instruction next, Pulse pulse, Instruction sender)>();
        }

        State = !State;
        
        var nextPulses = new List<(Instruction next, Pulse pulse, Instruction sender)>();
        foreach (var outgoingInstruction in OutgoingInstructions) {
            nextPulses.Add((outgoingInstruction, State ? Pulse.High : Pulse.Low, this));
        }
        return nextPulses;
    }

    private List<(Instruction next, Pulse pulse, Instruction sender)> DoInstructionConjunction(Pulse pulse, Instruction sender) {
        CountPulse(pulse);

        Debug.Assert(IncomingInstructions.ContainsKey(sender), $"Sender missing!");
        IncomingInstructions[sender] = pulse;

        var pulseToSend = Pulse.Low;
        foreach (var value in IncomingInstructions.Values) {
            if (value != Pulse.Low) continue;
            pulseToSend = Pulse.High;
            break;
        }

        var nextPulses = new List<(Instruction next, Pulse pulse, Instruction sender)>();
        foreach (var outgoingInstruction in OutgoingInstructions) {
            nextPulses.Add((outgoingInstruction, pulseToSend, this));
        }
        return nextPulses;
        
    }

    private List<(Instruction next, Pulse pulse, Instruction sender)> DoInstructionSink(Pulse pulse, Instruction sender) {
        CountPulse(pulse);
        // Console.WriteLine("<-End");
        return new List<(Instruction next, Pulse pulse, Instruction sender)>();
    }

    private List<(Instruction next, Pulse pulse, Instruction sender)> DoInstructionBroadcast(Pulse pulse, Instruction sender) {
        var nextPulses = new List<(Instruction next, Pulse pulse, Instruction sender)>();
        foreach (var outgoingInstruction in OutgoingInstructions) {
            nextPulses.Add((outgoingInstruction, pulse, this));
        }
        return nextPulses;
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
