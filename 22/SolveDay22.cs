using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d22y2023;

public static partial class Day22 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay22_00.txt", 5, null)
    };

    private const long ActualResult1 = 439;
    private const long ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0;
        result2 = 0;

        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        
        var bricks = new List<Brick>();
        var maxCoords = new Vector3Int(0, 0, 0);
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            var lineCoords = line.Split('~').Select(s => s.Split(',').Select(int.Parse).ToArray()).Select(coords => new Vector3Int(coords[0], coords[2], coords[1])).ToArray();
            var startCoords = lineCoords[0];
            var endCoords = lineCoords[1];
            bricks.Add(new Brick(startCoords, endCoords));
            maxCoords = new Vector3Int(Math.Max(maxCoords.X, Math.Max(startCoords.X, endCoords.X)), Math.Max(maxCoords.Y, Math.Max(startCoords.Y, endCoords.Y)), Math.Max(maxCoords.Z, Math.Max(startCoords.Z, endCoords.Z)));
        }

        var bricksGrid = new Brick?[maxCoords.X + 1, maxCoords.Y + 1, maxCoords.Z + 1];
        foreach (var brick in bricks) {
            for (int x = brick.StartCoords.X; x <= brick.EndCoords.X; x++) {
                for (int y = brick.StartCoords.Y; y <= brick.EndCoords.Y; y++) {
                    for (int z = brick.StartCoords.Z; z <= brick.EndCoords.Z; z++) {
                        Debug.Assert(bricksGrid[x, y, z] == null, $"Brick {brick.StartCoords} - {brick.EndCoords} overlaps with brick {bricksGrid[x, y, z]!.StartCoords} - {bricksGrid[x, y, z]!.EndCoords}!");
                        bricksGrid[x, y, z] = brick;
                    }
                }
            }
        }

        // Set bricks above and below
        foreach (var brick in bricks) {
            var yMin = brick.StartCoords.Y;
            var yMax = brick.EndCoords.Y;
            for (int x = brick.StartCoords.X; x <= brick.EndCoords.X; x++) {
                for (int z = brick.StartCoords.Z; z <= brick.EndCoords.Z; z++) {
                    for (int y = yMin - 1; y > 0; y--) {
                        if (bricksGrid[x, y, z] == null) continue;
                        brick.BricksBelow.Add(bricksGrid[x, y, z]!);
                        bricksGrid[x, y, z]!.BricksAbove.Add(brick);
                        break;
                    }
                    for (int y = yMax + 1; y <= maxCoords.Y; y++) {
                        if (bricksGrid[x, y, z] == null) continue;
                        brick.BricksAbove.Add(bricksGrid[x, y, z]!);
                        bricksGrid[x, y, z]!.BricksBelow.Add(brick);
                        break;
                    }
                }
            }
        }
        
        PrintGrid(bricksGrid, Rotation.X);
        PrintGrid(bricksGrid, Rotation.Z);
        
        var hasMoved = true;
        while (hasMoved) {
            Console.WriteLine("Moving bricks down...");
            hasMoved = false;
            foreach (var brick in bricks) {
                if (brick.MoveBrickDown(bricksGrid, false)) {
                    hasMoved = true;
                    // Console.WriteLine($"Moved brick {brick.Name} down to {brick.StartCoords.Y}!");
                }
                else {
                    // Console.WriteLine($"Brick {brick.Name} cannot be moved down!");
                }

            }
        }
        
        PrintGrid(bricksGrid, Rotation.X);
        PrintGrid(bricksGrid, Rotation.Z);

        foreach (var brick in bricks) {
            if (!brick.IsSingleSupportingBrick()) {
                // Console.WriteLine($"Brick {brick.Name} is not single supporting!");
                result1++;
            }
        }
    }
    
    public static void PrintGrid(Brick?[,,] bricksGrid, Rotation fromSide) {
        foreach (var brick in bricksGrid.Cast<Brick?>().Distinct()) {
            if (brick == null)
                continue;
            // Console.WriteLine($"Brick {brick.Name} ({brick.StartCoords} - {brick.EndCoords}), below: {string.Join(", ", brick.BricksBelow.Select(b => b?.Name.ToString() ?? "null"))}, above: {string.Join(", ", brick.BricksAbove.Select(b => b?.Name.ToString() ?? "null"))}");
        }
        Debug.Assert(fromSide != Rotation.Y, "Cannot print grid from side Y!");
        var height = bricksGrid.GetLength(1);
        var width = bricksGrid.GetLength(fromSide == Rotation.X ? 2 : 0);
        var depth = bricksGrid.GetLength(fromSide == Rotation.Z ? 2 : 0);
        for (int y = height - 1; y > 0; y--) {
            for (int side = 0; side < width; side++) {
                var brickName = ' ';
                for (int d = 0; d < depth; d++) {
                    var x = fromSide == Rotation.X ? side : d;
                    var z = fromSide == Rotation.Z ? side : d;
                    var brick = bricksGrid[x, y, z];
                    if (brick == null)
                        continue;
                    brickName = (brickName == ' ' || brickName == brick.Name) ? brick.Name : '?';
                }
                Console.Write(brickName);
            }
            Console.WriteLine();
        }
        Console.WriteLine("-----");
    }
}

public class Brick {
    public Vector3Int StartCoords { get; private set; }
    public Vector3Int EndCoords { get; private set; }
    private Vector3Int Dimensions { get; set; }
    public int Length { get; }
    public int HorizontalLength { get; }
    public Rotation Rotation { get; }
    
    public HashSet<Brick> BricksAbove { get; } = new();
    public HashSet<Brick> BricksBelow { get; } = new();
    
    public static Vector3Int MaxCoords { get; private set; } = new(0, 0, 0);
    public char Name { get; }
    public int Id { get; }
    private static int NextId { get; set; } = 0;

    public Brick(Vector3Int startCoords, Vector3Int endCoords) {
        Debug.Assert(startCoords.X >= 0 && endCoords.X >= 0, $"Brick {startCoords} - {endCoords} is left of 0!");
        Debug.Assert(startCoords.Z >= 0 && endCoords.Z >= 0, $"Brick {startCoords} - {endCoords} is in front of 0!");
        Debug.Assert(startCoords.Y > 0 && endCoords.Y > 0, $"Brick {startCoords} - {endCoords} is below the ground!");
        StartCoords = new Vector3Int(Math.Min(startCoords.X, endCoords.X), Math.Min(startCoords.Y, endCoords.Y), Math.Min(startCoords.Z, endCoords.Z));
        EndCoords = new Vector3Int(Math.Max(startCoords.X, endCoords.X), Math.Max(startCoords.Y, endCoords.Y), Math.Max(startCoords.Z, endCoords.Z));
        MaxCoords = new Vector3Int(Math.Max(MaxCoords.X, EndCoords.X), Math.Max(MaxCoords.Y, EndCoords.Y), Math.Max(MaxCoords.Z, EndCoords.Z));
        Length = Math.Abs(startCoords.X - endCoords.X) + Math.Abs(startCoords.Y - endCoords.Y) + Math.Abs(startCoords.Z - endCoords.Z) + 1;
        Debug.Assert(Length == Math.Abs(startCoords.X - endCoords.X) + 1 || Length == Math.Abs(startCoords.Y - endCoords.Y) + 1 || Length == Math.Abs(startCoords.Z - endCoords.Z) + 1, $"Brick {startCoords} - {endCoords} is not a straight line!");
        Rotation = Length == Math.Abs(startCoords.X - endCoords.X) + 1 ? Rotation.X : Length == Math.Abs(startCoords.Y - endCoords.Y) + 1 ? Rotation.Y : Rotation.Z;
        HorizontalLength = Rotation == Rotation.Y ? 1 : Length;
        Dimensions = new Vector3Int(Math.Abs(startCoords.X - endCoords.X), Math.Abs(startCoords.Y - endCoords.Y), Math.Abs(startCoords.Z - endCoords.Z));
        
        Id = NextId++;
        Name = (char) ('A' + Id);
    }
    
    public bool MoveBrickDown(Brick?[,,] bricks, bool moveBricksAbove) {
        if (StartCoords.Y == 1 || EndCoords.Y == 1) {
            return false;
        }
        Debug.Assert(StartCoords.Y > 1 && EndCoords.Y > 1, $"Brick {StartCoords} - {EndCoords} is below the ground!");

        var movable = int.MaxValue;
        foreach (var brickBelow in BricksBelow) {
            movable = Math.Min(movable, StartCoords.Y - brickBelow.EndCoords.Y - 1);
        }
        if (movable == 0) {
            return false;
        }
        if (movable == int.MaxValue) {
            movable = StartCoords.Y - 1;
        }

        for (int x = StartCoords.X; x <= EndCoords.X; x++) {
            for (int y = StartCoords.Y; y <= EndCoords.Y; y++) {
                for (int z = StartCoords.Z; z <= EndCoords.Z; z++) {
                    bricks[x, y, z] = null;
                }
            }
        }
        StartCoords.Y -= movable;
        EndCoords.Y -= movable;
        for (int x = StartCoords.X; x <= EndCoords.X; x++) {
            for (int y = StartCoords.Y; y <= EndCoords.Y; y++) {
                for (int z = StartCoords.Z; z <= EndCoords.Z; z++) {
                    Debug.Assert(x >= 0 && y >= 0 && z >= 0, $"Brick {StartCoords} - {EndCoords} is left of 0!");
                    Debug.Assert(x < bricks.GetLength(0) && y < bricks.GetLength(1) && z < bricks.GetLength(2), $"Brick {Name} {StartCoords} - {EndCoords} is right of {bricks.GetLength(0)}, {bricks.GetLength(1)}, {bricks.GetLength(2)}! moving {movable} down. Bricks below: {string.Join(", ", BricksBelow.Select(b => b?.Name.ToString() ?? "null"))}");
                    bricks[x, y, z] = this;
                }
            }
        }
        
        if (moveBricksAbove) {
            foreach (var brickAbove in BricksAbove) {
                brickAbove.MoveBrickDown(bricks, true);
            }
        }
        
        return true;
    }

    public bool IsSingleSupportingBrick() {
        foreach (var brickAbove in BricksAbove) {
            if (brickAbove == null || brickAbove.StartCoords.Y - 1 != EndCoords.Y) {
                continue;
            }
            if (brickAbove.SupportCount() == 1) {
                return true;
            }
        }

        return false;
    }

    private int SupportCount() {
        var count = 0;
        foreach (var brickBelow in BricksBelow) {
            if (brickBelow != null && brickBelow.EndCoords.Y == StartCoords.Y - 1) {
                count++;
            }
        }
        return count;
    }
}

public enum Rotation {
    X, Y, Z
}

public class Vector3Int {
    public int X { get; }
    public int Y { get; set; }
    public int Z { get; }
    
    public Vector3Int(int x, int y, int z) {
        X = x;
        Y = y;
        Z = z;
    }
    
    public override string ToString() {
        return $"({X}, {Y}, {Z})";
    }
}