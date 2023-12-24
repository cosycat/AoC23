using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

namespace d24y2023;

public static partial class Day24 {
    private static readonly List<(string fileName, long? expectedResult1, long? expectedResult2)> Tests = new() {
        ("testInputDay24_00.txt", 2, null)
    };

    private const long ActualResult1 = 0; // TODO replace
    private const long ActualResult2 = 0; // TODO replace
    
    private const bool ContinueIfTestsFail = false;

    public const double Epsilon = 1e-6f;

    private static void Solve(string inputFileName, out long result1, out long result2, bool isTest) {
        result1 = 0;
        result2 = 0;

        var inputLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(inputLines.Count > 0, $"Input file {inputFileName} is empty!");
        
        var allLines = new List<Line>();
        
        for (int i = 0; i < inputLines.Count; i++) {
            var inputLine = inputLines[i].Split(" @ ").Select(s => s.Split(", ")).ToList();
            var pos = inputLine[0];
            var vel = inputLine[1];
            
            Vector3 posVector = new(double.Parse(pos[0]), double.Parse(pos[1]), double.Parse(pos[2]));
            Vector3 velVector = new(double.Parse(vel[0]), double.Parse(vel[1]), double.Parse(vel[2]));
            Vector3 nextPosVector = posVector + velVector;
            allLines.Add(new Line (posVector, nextPosVector, velVector));
        }

        var minXY = isTest ? 7 : 200000000000000;
        var maxXY = isTest ? 27 : 400000000000000;

        for (var i = 0; i < allLines.Count; i++) {
            var line = allLines[i];
            Console.WriteLine($"Line {line.Name}: {line.A} -> {line.B} (Vel: {line.Vel})");
            for (var j = i + 1; j < allLines.Count; j++) {
                var otherLine = allLines[j];
                if (line == otherLine) continue;
                if (Intersection(line.A2D, line.B2D, otherLine.A2D, otherLine.B2D, out var intersectionParams,
                        out var outside, out var onPoint, out var intersectionPoint, out var inPast)) { 
                    if (inPast) continue;
                    Console.WriteLine($"Intersection with {otherLine.Name}: {intersectionPoint} (Params: {intersectionParams})");
                    if (intersectionPoint.X >= minXY && intersectionPoint.X <= maxXY && intersectionPoint.Y >= minXY &&
                        intersectionPoint.Y <= maxXY) {
                        Console.WriteLine($"Intersection in range: {intersectionPoint}");
                        result1++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the intersection point of two line segments
    /// </summary>
    /// <param name="A"> The first point of the first line segment </param>
    /// <param name="B"> The second point of the first line segment </param>
    /// <param name="C"> The first point of the second line segment </param>
    /// <param name="D"> The second point of the second line segment </param>
    /// <param name="intersectionParameters"> The lambda parameters for the intersection point, if there is one, otherwise default.
    /// The first parameter is for the first line segment, the second for the second line segment </param>
    /// <param name="outside"> True if the intersection point is outside the line segment, false otherwise </param>
    /// <param name="onPoint"> True if the intersection point is on one of the line segment points, false otherwise </param>
    /// <param name="intersectionPoint"> The intersection point if there is one, otherwise default </param>
    /// <param name="inPast"> True if the intersection point is in the past, false otherwise </param>
    /// <returns> True if there is an intersection, false otherwise </returns>
    public static bool Intersection(Vector2 A, Vector2 B, Vector2 C, Vector2 D,
        out Vector2 intersectionParameters, out bool outside, out bool onPoint, out Vector2 intersectionPoint, out bool inPast) {
        var AB = B - A;
        var CD = D - C;
        var det = AB.Y * CD.X - AB.X * CD.Y;
        if (Math.Abs(det) < Epsilon) {
            intersectionParameters = default;
            outside = false;
            onPoint = false;
            intersectionPoint = default;
            inPast = false;
            return false;
        }
        var AC = C - A;

        intersectionParameters = 1 / det * new Vector2(
            -CD.Y * AC.X + CD.X * AC.Y,
            -AB.Y * AC.X + AB.X * AC.Y
        );
        outside = intersectionParameters.X < 0 || intersectionParameters.X > 1 || intersectionParameters.Y < 0 || intersectionParameters.Y > 1;
        inPast = intersectionParameters.X < 0 || intersectionParameters.Y < 0;
        onPoint = Math.Abs(intersectionParameters.X) < Epsilon || Math.Abs(intersectionParameters.X - 1) < Epsilon ||
                   Math.Abs(intersectionParameters.Y) < Epsilon || Math.Abs(intersectionParameters.Y - 1) < Epsilon;
        intersectionPoint = A + intersectionParameters.X * AB;
        
        // Debug.Assert(Math.Abs(intersectionPoint.X - (C.X + intersectionParameters.Y * CD.X)) < Epsilon, "Intersections don't match (X Axis)");
        // Debug.Assert(Math.Abs(intersectionPoint.Y - (C.Y + intersectionParameters.Y * CD.Y)) < Epsilon, "Intersections don't match (Y Axis)");
        return true;
    }
    
    /// <summary>
    /// Calculates the intersection point of two line segments in 3D
    /// Uses <see cref="Intersection(Vector2,Vector2,Vector2,Vector2,out Vector2,out bool,out bool,out Vector2)"/> for the 2D intersection,
    /// and den checks if the intersection point is on the same Z axis.
    /// If there is no intersection, all the out parameters are undefined.
    /// </summary>
    /// <param name="A"> The first point of the first line segment </param>
    /// <param name="B"> The second point of the first line segment </param>
    /// <param name="C"> The first point of the second line segment </param>
    /// <param name="D"> The second point of the second line segment </param>
    /// <param name="intersectionParams"> The lambda parameters for the intersection point, if there is one, otherwise default.
    /// The first parameter is for the first line segment, the second for the second line segment </param>
    /// <param name="outside"> True if the intersection point is outside the line segment, false otherwise </param>
    /// <param name="onPoint"> True if the intersection point is on one of the line segment points, false otherwise </param>
    /// <param name="intersectionPoint"> The intersection point if there is one, otherwise default </param>
    /// <returns> True if there is an intersection, false otherwise </returns>
    public static bool Intersection3D(Vector3 A, Vector3 B, Vector3 C, Vector3 D,
        out Vector2 intersectionParams, out bool outside, out bool onPoint, out Vector3 intersectionPoint) {
        if (!Intersection(new Vector2(A.X, A.Y), new Vector2(B.X, B.Y), new Vector2(C.X, C.Y), new Vector2(D.X, D.Y), out intersectionParams, out outside, out onPoint, out var intersectionPoint2D, out var inPast)) {
            intersectionPoint = default;
            return false;
        }

        var intersectionZAB = (1 - intersectionParams.X) * A.Z + intersectionParams.X * B.Z;
        var intersectionZCD = (1 - intersectionParams.Y) * C.Z + intersectionParams.Y * D.Z;
        intersectionPoint = new Vector3(intersectionPoint2D, (intersectionZAB + intersectionZCD) / 2);
        return Math.Abs(intersectionZAB - intersectionZCD) < Epsilon;
    }
}

internal class Line {
    private static char nextName = 'A';
    public string Name { get; }
    public Vector3 A { get; }
    public Vector3 B { get; }
    public Vector3 Vel { get; }
    public Vector2 A2D { get; }
    public Vector2 B2D { get; }
    public Vector2 Vel2D { get; }

    public Line(Vector3 a, Vector3 b, Vector3 vel) {
        A = a;
        B = b;
        Vel = vel;
        A2D = new Vector2(A.X, A.Y);
        B2D = new Vector2(B.X, B.Y);
        Vel2D = new Vector2(Vel.X, Vel.Y);
        Name = nextName++.ToString();
    }
}

public class Vector3 {
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3(double x, double y, double z) {
        X = x;
        Y = y;
        Z = z;
    }
    
    public Vector3(Vector2 v, double z) {
        X = v.X;
        Y = v.Y;
        Z = z;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(double a, Vector3 b) => new(a * b.X, a * b.Y, a * b.Z);
    public static Vector3 operator *(Vector3 a, double b) => new(a.X * b, a.Y * b, a.Z * b);
    public static Vector3 operator /(Vector3 a, double b) => new(a.X / b, a.Y / b, a.Z / b);
    public static bool operator ==(Vector3 a, Vector3 b) => Math.Abs(a.X - b.X) < Day24.Epsilon && Math.Abs(a.Y - b.Y) < Day24.Epsilon && Math.Abs(a.Z - b.Z) < Day24.Epsilon;
    public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);

    public override bool Equals(object? obj) {
        if (obj is Vector3 v) {
            return this == v;
        }
        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y, Z);
    }

    public override string ToString() {
        return $"({X}, {Y}, {Z})";
    }
}

public class Vector2 {
    public double X { get; }
    public double Y { get; }

    public Vector2(double x, double y) {
        X = x;
        Y = y;
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(double a, Vector2 b) => new(a * b.X, a * b.Y);
    public static Vector2 operator *(Vector2 a, double b) => new(a.X * b, a.Y * b);
    public static Vector2 operator /(Vector2 a, double b) => new(a.X / b, a.Y / b);
    public static bool operator ==(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < Day24.Epsilon && Math.Abs(a.Y - b.Y) < Day24.Epsilon;
    public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

    public override bool Equals(object? obj) {
        if (obj is Vector2 v) {
            return this == v;
        }
        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }

    public override string ToString() {
        return $"({X}, {Y})";
    }
}
