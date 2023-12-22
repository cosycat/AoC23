using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d21y2023;

public class Arguments {
    public int MaxDistance { get; set; }
}

public static partial class Day21 {
    private static readonly List<(string fileName, long? expectedResult1, Arguments arguments)> Tests = new() {
        ("testInputDay21_00.txt", 16, new Arguments {MaxDistance = 6}),
        ("testInputDay21_00.txt", 50, new Arguments {MaxDistance = 10}),
        ("testInputDay21_00.txt", 1594, new Arguments {MaxDistance = 50}),
        ("testInputDay21_00.txt", 6536, new Arguments {MaxDistance = 100}),
        ("testInputDay21_00.txt", 167004, new Arguments {MaxDistance = 150}),
        ("testInputDay21_00.txt", 668697, new Arguments {MaxDistance = 1000}),
        ("testInputDay21_00.txt", 16733044, new Arguments {MaxDistance = 5000}),
    };

    private const long ActualResult1 = 3764;
    private const long ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result, Arguments arguments) {
        result = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var width = allLines[0].Length;
        var height = allLines.Count;
        
        var grid = new Tile[height, width];
        
        var start = (x: 0, y: 0);

        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                var pos = (x, y);
                switch (c) {
                    case '.':
                        grid[y, x] = new Tile(pos);
                        break;
                    case '#':
                        grid[y, x] = new Tile(pos) { IsRock = true };
                        break;
                    case 'S':
                        grid[y, x] = new Tile(pos) { Visited = VisitedType.Even };
                        start = (x, y);
                        break;
                    default:
                        throw new Exception($"Unknown char {c}");
                }
            }
        }

        var tilesReachable = new HashSet<Tile>();
        
        var next = new Queue<(int x, int y, int steps)>();
        next.Enqueue((start.x, start.y, 0));
        var maxDistance = arguments.MaxDistance;
        Console.WriteLine($"maxDistance: {maxDistance}");
        while (next.Count > 0) {
            var current = next.Dequeue();
            Debug.Assert(current.steps < maxDistance2, "Steps are too high!");
            var neighbours = grid[current.y, current.x].GetNeighbours(grid);
            foreach (var neighbour in neighbours) {
                // Debug.Assert(neighbour == null || neighbour.Visited == false || neighbour.Distance <= current.Distance + 1, "Distance is not increasing!");
                if (neighbour == null || neighbour.IsRock) {
                    continue;
                }
                
                // neighbour.Visited = true;
                if (!next.Contains((neighbour.Position.x, neighbour.Position.y, current.steps + 1))) {
                    next.Enqueue((neighbour.Position.x, neighbour.Position.y, current.steps + 1));
                }
                Debug.Assert(next.Count(t => t == (neighbour.Position.x, neighbour.Position.y, current.steps + 1)) == 1, $"Queue contains {neighbour.Position.x}, {neighbour.Position.y}, {current.steps + 1} {next.Count(t => t == (neighbour.Position.x, neighbour.Position.y, current.steps + 1))} times");
            }
        }
        
        result = tilesReachable1.Count;
        result2 = tilesReachable2.Count;
        
        PrintGrid(grid);
    }

    private static (Dictionary<int,List<(int x, int y)>> reachableTiles, Dictionary<Direction, List<((int x, int y) position, int steps)>> edgeTiles) FillGridFrom((int x, int y) startPos) {
        
    }

    private static void PrintGrid(Tile[,] grid) {
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        for (int y = 0; y < height; y++) {
            var line = "";
            for (int x = 0; x < width; x++) {
                var c = grid[y, x] switch {
                    { IsRock: true } => '#',
                    { Visited: true } => '1',
                    { Visited2: true } => '2',
                    _ => '.'
                };
                line += c;
            }

            Console.WriteLine(line);
        }
    }
}

public class Tile {
    public Tile((int x, int y) pos) {
        Position = pos;
    }

    public bool IsRock { get; set; }
    public VisitedType Visited { get; set; } = VisitedType.None;

    public (int x, int y) Position { get; set; }
    
    public IEnumerable<(int x, int y, Direction largerGrid)> GetNeighbours(Tile[,] grid) {
        var width = grid.GetLength(1);
        var height = grid.GetLength(0);
        var (x, y) = Position;
        var neighbours = new List<(int x, int y, Direction largerGrid)> {
            (x - 1, y, x - 1 < 0 ? Direction.Left : Direction.Center),
            (x + 1, y, x + 1 >= width ? Direction.Right : Direction.Center),
            (x, y - 1, y - 1 < 0 ? Direction.Up : Direction.Center),
            (x, y + 1, y + 1 >= height ? Direction.Down : Direction.Center)
        };
        return neighbours;
    }
}

public enum VisitedType {
    None,
    Even,
    Odd
}

public enum Direction {
    Up,
    Down,
    Left,
    Right,
    Center
}
