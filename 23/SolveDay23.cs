using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d23y2023;

public static partial class Day23 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay23_00.txt", 94, 154)
    };

    private const long ActualResult1 = 2018;
    private const long ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static readonly (int dx, int dy)[] NeighbourDxDy = {(0, 1), (0, -1), (1, 0), (-1, 0)};
    private const int NoArea = -1;
    private const int PointArea = -2;


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
        result1 = FindLongestPath(startPos, endPos, width, height, grid, useSlopes: true);
        
        var (areaGrid, pointAreas) = FindAreas(grid, width, height, startPos, endPos);
        
        var startArea = areaGrid[startPos.y, startPos.x].area;
        var endArea = areaGrid[endPos.y, endPos.x].area;
        pointAreas.Add((startPos.x, startPos.y, new List<int>{startArea}));
        pointAreas.Add((endPos.x, endPos.y, new List<int>{endArea}));
        areaGrid[startPos.y, startPos.x] = (areaGrid[startPos.y, startPos.x].t, PointArea);
        areaGrid[endPos.y, endPos.x] = (areaGrid[endPos.y, endPos.x].t, PointArea);
        
        var areaConnections = new Dictionary<(int x, int y), List<(int x, int y, long maxDistance)>>();
        foreach (var pointArea in pointAreas) {
            var pointNeighbours = new List<(int x, int y, long maxDistance)>();
            
            foreach (var otherPoint in pointAreas) { // Optimization: don't check both directions
                if (otherPoint == pointArea)
                    continue;
                var commonAreas = pointArea.areas.Intersect(otherPoint.areas).ToList();
                if (!commonAreas.Any()) continue; // the points don't share an area
                Debug.Assert(commonAreas.Count == 1, $"Points at ({pointArea.x},{pointArea.y}) and ({otherPoint.x},{otherPoint.y}) share more than one area");
                // the points share an area
                var commonArea = commonAreas[0];
                Debug.Assert(commonArea >= 0, $"Points at ({pointArea.x},{pointArea.y}) and ({otherPoint.x},{otherPoint.y}) share an invalid area {commonArea}");

                Console.Write($"Found connection between ({pointArea.x},{pointArea.y}) and ({otherPoint.x},{otherPoint.y}) with area {commonArea}...");
                var neighbour1 = GetNeighbours(areaGrid, pointArea.x, pointArea.y, width, height).Find(n => areaGrid[n.y, n.x].area == commonArea);
                var neighbour2 = GetNeighbours(areaGrid, otherPoint.x, otherPoint.y, width, height).Find(n => areaGrid[n.y, n.x].area == commonArea);
                grid[pointArea.y, pointArea.x] = Tile.Wall;
                grid[otherPoint.y, otherPoint.x] = Tile.Wall;
                var distance = FindLongestPath(neighbour1, neighbour2, width, height, grid, useSlopes: false) + 2;
                pointNeighbours.Add((otherPoint.x, otherPoint.y, distance));
                grid[pointArea.x, pointArea.y] = Tile.Empty;
                grid[otherPoint.x, otherPoint.y] = Tile.Empty;
                Console.WriteLine($"->distance {distance}");
            }
            areaConnections.Add((pointArea.x, pointArea.y), pointNeighbours);
        }

        PrintAreaGrid(areaGrid, width, height);
        Console.WriteLine($"Found {areaConnections.Count} connections: {string.Join("; ", areaConnections.Select(p => $"({p.Key.x},{p.Key.y}) -> {string.Join(", ", p.Value.Select(p => $"({p.x},{p.y}:{p.maxDistance})"))}"))}");
        
        result2 = FindLongestPathGraph(areaConnections, startPos, endPos);
    }

    private static long FindLongestPathGraph(Dictionary<(int x, int y),List<(int x, int y, long maxDistance)>> areaConnections, (int x, int y) startPos, (int x, int y) endPos) {
        var result = 0L;
        var nextTiles = new List<(int x, int y, long distance, List<(int x, int y)> prevPath)>();
        nextTiles.Add((startPos.x, startPos.y, 0, new List<(int x, int y)>()));
        while (nextTiles.Count > 0) {
            var (x, y, distance, prevPath) = nextTiles[^1];
            nextTiles.RemoveAt(nextTiles.Count - 1);
            if (x == endPos.x && y == endPos.y) {
                if (result < distance) {
                    result = distance;
                    Console.WriteLine($"Found larger path with length {distance} along {string.Join(", ", prevPath.Select(p => $"({p.x},{p.y})"))}");
                    // PrintPath(prevPath, grid);
                }
                continue;
            }
            
            if (!areaConnections.ContainsKey((x, y)))
                continue;
            
            if (prevPath.Any(p => p.x == x && p.y == y)) {
                continue; // Cycle
            }

            foreach (var (newX, newY, newDist) in areaConnections[(x, y)]) {
                var newPath = prevPath.ToList();
                newPath.Add((x, y));
                nextTiles.Add((newX, newY, distance + newDist, newPath));
            }
        }
        
        return result;
    }

    private static ((Tile t, int area)[,] areaGrid, List<(int x, int y, List<int> areas)> pointAreas) FindAreas(Tile[,] grid, int width, int height, (int x, int y) startPos, (int x, int y) endPos) {
        var areaGrid = new (Tile t, int area)[height, width];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) { 
                areaGrid[y, x] = (grid[y, x], -1);
            }
        }
        var points = new List<(int x, int y)>();
        // points.Add(startPos);
        // var areas = new List<((int x, int y) start, List<(int x, int y)> initialPath, (int x, int y) end)>();
        var nextTiles = new Queue<(int x, int y, int area)>();
        nextTiles.Enqueue((startPos.x, startPos.y + 1, 0));
        var nextArea = 1;
        var equalAreas = new List<(int area1, int area2)>();
        while (nextTiles.Count > 0) {
            var (x, y, area) = nextTiles.Dequeue();
            // PrintAreaGrid(areaGrid, width, height);
            // Debug.Assert(areaGrid[y, x].area == noArea || areaGrid[y, x].area == area, $"Area mismatch at ({x},{y}): {areaGrid[y, x].area} != {area}");
            if (areaGrid[y, x].area != NoArea && areaGrid[y, x].area != area) {
                // already visited from another direction with another area
                // Debug.Assert(areaGrid[y, x].area >= 0 && area >=0, $"Invalid area {areaGrid[y, x].area} at ({x},{y}) or {area} at ({x},{y})");
                equalAreas.Add((areaGrid[y, x].area, area));
                continue;
            }
            Debug.Assert(areaGrid[y, x].t != Tile.Wall, $"Wall at ({x},{y})");
            var isInOpenSpace = CountNeighbors(areaGrid, x, y, width, height) > 2;
            if (!isInOpenSpace) {
                // still same area
                areaGrid[y, x] = (areaGrid[y, x].t, area);
                Debug.Assert(area >= 0);
            }
            else {
                // new area
                areaGrid[y, x] = (areaGrid[y, x].t, PointArea);
                points.Add((x, y));
                Console.WriteLine($"Found point at ({x},{y})");
            }

            foreach (var (dx, dy) in NeighbourDxDy) {
                var (newX, newY) = (x + dx, y + dy);
                if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                    continue;
                if (areaGrid[newY, newX].t == Tile.Wall)
                    continue;
                if (areaGrid[newY, newX].area == PointArea) {
                    Console.WriteLine($"Two points next to each other at ({newX},{newY})");
                    continue;
                }

                if (areaGrid[newY, newX].area != NoArea)
                    continue;
                nextTiles.Enqueue((newX, newY, isInOpenSpace ? nextArea++ : area)); // new area if in open space
            }
        }

        Console.WriteLine($"Found {points.Count} points: {string.Join(", ", points.Select(p => $"({p.x},{p.y})"))}");

        Console.WriteLine($"Found {equalAreas.Count} equal areas: {string.Join("; ", equalAreas.Select(p => $"({(char)('A' + p.area1)},{(char)('A' + p.area2)})"))}");
        foreach (var equalArea in equalAreas) {
            if (equalArea.area1 == PointArea || equalArea.area2 == PointArea)
                continue;
            Debug.Assert(equalArea is { area1: >= 0, area2: >= 0 }, $"Invalid area {equalArea.area1} or {equalArea.area2}");
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) { 
                    if (areaGrid[y, x].area == equalArea.area2) {
                        Debug.Assert(equalArea.area1 >= 0, $"Invalid area {equalArea.area1}");
                        areaGrid[y, x] = (areaGrid[y, x].t, equalArea.area1);
                    }
                }
            }
        }
        
        PrintAreaGrid(areaGrid, width, height);

        var pointAreas = new List<(int x, int y, List<int> areas)>();
        Debug.Assert(points.Count == points.Distinct().Count(), $"Duplicate points found: {string.Join(", ", points.Select(p => $"({p.x},{p.y})"))}");
        foreach (var point in points) {
            var neighbours = GetNeighbours(areaGrid, point.x, point.y, width, height);
            Debug.Assert(neighbours.Select(n => areaGrid[n.y, n.x].area).Distinct().Count() == neighbours.Count, $"Point at ({point.x},{point.y}) has neighbours with same area");
            Debug.Assert(neighbours.Count > 2, $"Point at ({point.x},{point.y}) has {neighbours.Count} neighbours");
            pointAreas.Add((point.x, point.y, neighbours.Select(n => areaGrid[n.y, n.x].area).Distinct().ToList()));
        }
        
        return (areaGrid, pointAreas);
    }

    private static List<(int x, int y)> GetNeighbours((Tile t, int area)[,] areaGrid, int x, int y, int width, int height) {
        var result = new List<(int x, int y)>();
        foreach (var (dx, dy) in NeighbourDxDy) {
            var (newX, newY) = (x + dx, y + dy);
            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                continue;
            if (areaGrid[newY, newX].t == Tile.Wall)
                continue;
            result.Add((newX, newY));
        }

        return result;
    }

    private static int CountNeighbors((Tile t, int area)[,] areaGrid, int x, int y, int width, int height) {
        var result = 0;
        foreach (var (dx, dy) in NeighbourDxDy) {
            var (newX, newY) = (x + dx, y + dy);
            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                continue;
            if (areaGrid[newY, newX].t == Tile.Wall)
                continue;
            result++;
        }

        return result;
    }

    private static long FindLongestPath((int x, int y) startPos, (int x, int y) endPos, int width, int height, Tile[,] grid, bool useSlopes) {
        Debug.Assert(startPos.x >= 0 && startPos.x < width && startPos.y >= 0 && startPos.y < height, $"Invalid start position ({startPos.x},{startPos.y})");
        Debug.Assert(endPos.x >= 0 && endPos.x < width && endPos.y >= 0 && endPos.y < height, $"Invalid end position ({endPos.x},{endPos.y})");
        Debug.Assert(grid[startPos.y, startPos.x] != Tile.Wall, $"Invalid start tile {grid[startPos.y, startPos.x]} at ({startPos.x},{startPos.y})");
        Debug.Assert(grid[endPos.y, endPos.x] != Tile.Wall, $"Invalid end tile {grid[endPos.y, endPos.x]} at ({endPos.x},{endPos.y})");
        Debug.Assert(startPos.x != endPos.x || startPos.y != endPos.y, $"Start and end position are the same ({startPos.x},{startPos.y})");
        var result = 0L;
        var nextTiles = new List<(int x, int y, int distance, List<(int x, int y)> prevPath)>();
        nextTiles.Add((startPos.x, startPos.y, 0, new List<(int x, int y)>()));
        while (nextTiles.Count > 0) {
            var (x, y, distance, prevPath) = nextTiles[^1];
            nextTiles.RemoveAt(nextTiles.Count - 1);
            if (x == endPos.x && y == endPos.y) {
                if (result < distance) {
                    result = distance;
                    // Console.WriteLine($"Found path with length {distance}");
                    // PrintPath(prevPath, grid);
                }
                continue;
            }
            
            if (x < 0 || x >= width || y < 0 || y >= height)
                continue;
            if (grid[y, x] == Tile.Wall)
                continue;
            
            if (prevPath.Any(p => p.x == x && p.y == y)) {
                continue; // Cycle
            }

            foreach (var (dx, dy) in NeighbourDxDy) {
                var (newPos, newDist) = ((x: x + dx, y: y + dy), distance + 1);
                if (newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height)
                    continue;
                var tile = grid[newPos.y, newPos.x];
                var newPath = prevPath.ToList();
                newPath.Add((x, y));
                if (useSlopes) {
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
                }

                if (newPos.x < 0 || newPos.x >= width || newPos.y < 0 || newPos.y >= height)
                    continue;
                if (grid[newPos.y, newPos.x] == Tile.Wall)
                    continue;
                nextTiles.Add((newPos.x, newPos.y, newDist, newPath));
            }
        }

        if (result == 0) {
            Console.WriteLine();
            PrintPath(nextTiles.Select(t => (t.x, t.y)).ToList(), grid);
            Debug.Fail($"No path found from ({startPos.x},{startPos.y}) to ({endPos.x},{endPos.y})");
        }

        return result;
    }

    private static void PrintAreaGrid((Tile t, int area)[,] grid, int width, int height) {
        Console.Write("   ");
        for (int i = 0; i < width; i++) Console.Write(i % 10);
        Console.WriteLine();
        for (int y = 0; y < height; y++) {
            var line = $"{y,2} ";
            for (int x = 0; x < width; x++) {
                if (grid[y, x].t == Tile.Wall) {
                    line += '#';
                }
                else {
                    line += grid[y, x].area switch {
                        -1 => '.',
                        -2 => '❌',
                        _ => (char)('A' + grid[y, x].area)
                    };
                
                }
            }
            Console.WriteLine(line);
        }

        Console.WriteLine();
    }

    private static void PrintPath(List<(int x, int y)> prevPath, Tile[,] grid) {
        for (int y = 0; y < grid.GetLength(0); y++) {
            var line = "";
            for (int x = 0; x < grid.GetLength(1); x++) {
                if (prevPath.Any(p => p.x == x && p.y == y)) {
                    line += 'O';
                }
                else {
                    line += grid[y, x] switch {
                        Tile.Empty => '.',
                        Tile.SlopeDown => 'v',
                        Tile.SlopeUp => '^',
                        Tile.SlopeLeft => '<',
                        Tile.SlopeRight => '>',
                        Tile.Wall => '#',
                        _ => throw new Exception($"Unknown tile type {grid[y, x]} at ({x},{y})")
                    };
                }
            }
            Console.WriteLine(line);
        }

        Console.WriteLine();
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