using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d23y2023;

public static partial class Day23 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay23_00.txt", 94, null) // TODO replace
    };

    private const long ActualResult1 = 0; // TODO replace
    private const long ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var width = allLines[0].Length;
        var height = allLines.Count;
        
        var grid = new Tile[height, width];
        var startPos = (x: 0, y: 0);
        var endPos = (x: 0, y: 0);

        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                if (y == 0 && c == '.') {
                    startPos = (x, y);
                }
                else if (y == height - 1 && c == '.') {
                    endPos = (x, y);
                }

                grid[y, x] = c switch {
                    '#' => Tile.Wall,
                    '.' => Tile.Empty,
                    '>' => Tile.SlopeRight,
                    '<' => Tile.SlopeLeft,
                    'v' => Tile.SlopeDown,
                    '^' => Tile.SlopeUp,
                    _ => throw new Exception($"Unknown tile type {c} at ({x},{y})")
                };
            }
        }
        
        // find longest path length
        var nextTiles = new List<(int x, int y, int distance, List<(int x, int y)> prevPath)>();
        nextTiles.Add((startPos.x, startPos.y, 0, new List<(int x, int y)>()));
        while (nextTiles.Count > 0) {
            var (x, y, distance, prevPath) = nextTiles[^1];
            nextTiles.RemoveAt(nextTiles.Count - 1);
            if (x == endPos.x && y == endPos.y) {
                result1 = Math.Max(result1, distance);
                Console.WriteLine($"Found path with length {distance}");
                continue;
            }
            
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;
            if (grid[y, x] == Tile.Wall)
                continue;

            if (nextTiles.Any(t => t.x == x && t.y == y)) {
                var other = nextTiles.First(t => t.x == x && t.y == y);
                if (other.distance <= distance) {
                    nextTiles.Remove(other);
                }
                else {
                    continue;
                }
            }
            if (prevPath.Any(p => p.x == x && p.y == y)) {
                continue; // Cycle
            }

            foreach (var (dx, dy) in new[] {(0, 1), (0, -1), (1, 0), (-1, 0)}) {
                var (newPos, newDist) = ((x: x + dx, y: y + dy), distance + 1);
                if (newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height)
                    continue;
                var tile = grid[newPos.y, newPos.x];
                var newPath = prevPath.ToList();
                newPath.Add((x, y));
                if (tile == Tile.SlopeDown) {
                    newPos = (x + dx, y + dy + 1);
                    newDist++;
                    newPath.Add((x + dx, y + dy));
                }
                else if (tile == Tile.SlopeUp) {
                    newPos = (x + dx, y + dy - 1);
                    newDist++;
                    newPath.Add((x + dx, y + dy));
                }
                else if (tile == Tile.SlopeLeft) {
                    newPos = (x + dx - 1, y + dy);
                    newDist++;
                    newPath.Add((x + dx, y + dy));
                }
                else if (tile == Tile.SlopeRight) {
                    newPos = (x + dx + 1, y + dy);
                    newDist++;
                    newPath.Add((x + dx, y + dy));
                }
                if (newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height)
                    continue;
                if (grid[newPos.y, newPos.x] == Tile.Wall)
                    continue;
                nextTiles.Add((newPos.x, newPos.y, newDist, newPath));
            }
            
        }
        
        
    }
}

public enum Tile {
    Empty,
    SlopeUp,
    SlopeDown,
    SlopeLeft,
    SlopeRight,
    Wall
}