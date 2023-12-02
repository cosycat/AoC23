using System.Diagnostics;

namespace _2022_24; 

public static class Day24Year22 {

    public static void Main(string[] args) {
        
        var allLines = File.ReadAllLines("input.txt").ToList();
        var g = new Grid(allLines);

        Console.WriteLine("parsed input");

        PriorityQueue<(int x, int y, int steps), int> states = new PriorityQueue<(int x, int y, int steps), int>();
        states.Enqueue((g.StartX, 0, 0), 0);

        var totalCount = 0;
        while (states.Count > 0) {
            var state = states.Dequeue();
            totalCount++;
            if (totalCount % 100000 == 0) Console.WriteLine($"total count: {totalCount}, current state: {state}");
            
            foreach (var nextState in g.GetValidNextStates(state)) {
                Debug.Assert(nextState.steps == state.steps + 1, $"nextState.steps: {nextState.steps}, state.steps: {state.steps}");
                if (nextState.y == g.Height - 1) {
                    Console.WriteLine($"Minimum count of steps: {nextState.steps}");
                    return;
                }
                
                states.Enqueue(nextState, g.GetMinNumberOfSteps(nextState));
            }
        }

    }
    
}



public class Grid {
    
    public int Width { get; }
    public int Height { get; }

    public readonly Blizzard[,] Blizzards;

    public readonly int StartX;
    public readonly int GoalX;

    public Grid(List<string> input) {
        Width = input[0].Length;
        Height = input.Count;

        Blizzards = new Blizzard[Height, Width];

        for (int y = 0; y < input.Count; y++) {
            AddRow(input[y], y);
        }
        
        Debug.Assert(input[0].ToCharArray().ToList().FindAll(i => i == '.').Count == 1);
        Debug.Assert(input[^1].ToCharArray().ToList().FindAll(i => i == '.').Count == 1);
        for (int x = 0; x < Width; x++) {
            if (input[0][x] == '.') {
                StartX = x;
                Blizzards[0, x] = Blizzard.Wall;
            }
            if (input[^1][x] == '.') GoalX = x;
        }
    }

    private void AddRow(string s, int y) {
        Debug.Assert(s[0] == s[^1] && s[0] == '#');
        for (int x = 0; x < s.Length; x++) {
            switch (s[x]) {
                case '.':
                    Blizzards[y, x] = Blizzard.None;
                    break;
                case '>':
                    Blizzards[y, x] = Blizzard.Right;
                    break;
                case '<':
                    Blizzards[y, x] = Blizzard.Left;
                    break;
                case '^':
                    Blizzards[y, x] = Blizzard.Up;
                    break;
                case 'v':
                    Blizzards[y, x] = Blizzard.Down;
                    break;
                case '#':
                    Blizzards[y, x] = Blizzard.Wall;
                    break;
                default:
                    throw new Exception("wrong symbol");
            }
        }
    }

    public IEnumerable<(int x, int y, int steps)> GetValidNextStates((int x, int y, int steps) currState) {
        Debug.Assert(currState.y < Height - 1);
        var validNextStates = new List<(int x, int y, int steps)>();
        var nextStepCount = currState.steps + 1;
        if (currState.y == 0) {
            if (!IsBlocked(1, currState.x, nextStepCount))
                 validNextStates.Add((currState.x, 1, nextStepCount));
            else
                validNextStates.Add((currState.x, 0, nextStepCount));
            return validNextStates;
        }
        Debug.Assert(currState.y > 0 && currState.y < Height - 1 && currState.x > 0 && currState.x < Width - 1, $"Should never be on the border here! x: {currState.x}, y: {currState.y}");
        
        if (!IsBlocked(currState.y + 1, currState.x, nextStepCount))
            validNextStates.Add((currState.x, currState.y + 1, nextStepCount));
        if (!IsBlocked(currState.y - 1, currState.x, nextStepCount))
            validNextStates.Add((currState.x, currState.y - 1, nextStepCount));
        if (!IsBlocked(currState.y, currState.x + 1, nextStepCount))
            validNextStates.Add((currState.x + 1, currState.y, nextStepCount));
        if (!IsBlocked(currState.y, currState.x - 1, nextStepCount))
            validNextStates.Add((currState.x - 1, currState.y, nextStepCount));
        
        if (!IsBlocked(currState.y, currState.x, nextStepCount))
            validNextStates.Add((currState.x, currState.y, nextStepCount));

        return validNextStates;
        // for (int x = currState.x-1; x <= currState.x+1; x++) {
        //     for (int y = currState.y-1; y <= currState.y+1; y++) {
        //         if (IsBlocked(y, x, nextStepCount)) continue;
        //         yield return (x, y, nextStepCount);
        //     }
        // }
    }

    private bool IsBlocked(int y, int x, int i) {
        if (Blizzards[y, x] == Blizzard.Wall) return true;
        var enemyDownPos = (y - 1 + i).Mod(Height - 2) + 1;
        Debug.Assert(enemyDownPos >= 0 && enemyDownPos < Height, $"enemyDownPos: {enemyDownPos}, y: {y}, i: {i}, Height: {Height}");
        if (Blizzards[enemyDownPos, x] == Blizzard.Up) return true;
        var enemyUpPos = (y - 1 - i).Mod(Height - 2) + 1;
        Debug.Assert(enemyUpPos >= 0 && enemyUpPos < Height, $"enemyUpPos: {enemyUpPos}, y: {y}, i: {i}, Height: {Height}");
        if (Blizzards[enemyUpPos, x] == Blizzard.Down) return true;
        var enemyLeftPos = (x - 1 - i).Mod(Width - 2) + 1;
        Debug.Assert(enemyLeftPos >= 0 && enemyLeftPos < Width, $"enemyLeftPos: {enemyLeftPos}, x: {x}, i: {i}, Width: {Width}");
        if (Blizzards[y, enemyLeftPos] == Blizzard.Right) return true;
        var enemyRightPos = (x - 1 + i).Mod(Width - 2) + 1;
        Debug.Assert(enemyRightPos >= 0 && enemyRightPos < Width, $"enemyRightPos: {enemyRightPos}, x: {x}, i: {i}, Width: {Width}");
        if (Blizzards[y, enemyRightPos] == Blizzard.Left) return true;
        return false;
    }

    public int GetMinNumberOfSteps((int x, int y, int steps) nextState) {
        return nextState.steps + Math.Abs(nextState.x - GoalX) + Math.Abs(nextState.y - Height + 1);
    }

    public void PrintState((int x, int y, int steps) state) {
        Console.WriteLine($"x: {state.x}, y: {state.y}, steps: {state.steps}");
        for (int y = 0; y < Height; y++) {
            for (int x = 0; x < Width; x++) {
                if (state.x == x && state.y == y) {
                    Console.Write('E');
                } else {
                    Console.Write(IsBlocked(y, x, state.steps) ? '#' : '.');
                }
            }
            Console.WriteLine();
        }
    }
}

// from https://github.com/dotnet/csharplang/discussions/4744
public static class IntExtensions {
    public static int Mod(this int a, int b)
    {
        var c = a % b;
        if ((c < 0 && b > 0) || (c > 0 && b < 0))
        {
            c += b;
        }
        return c;
    }
}

public enum Blizzard {
    None,
    Up,
    Right,
    Down,
    Left,
    Wall,
}
