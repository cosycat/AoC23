using System.Diagnostics;

namespace Templates; 

public static class Dayxx {
    private static string InputFileName => "inputDayxx.txt";
    private static string TestFileName => "testInputDayxx.txt";
    public static /*resultType*/int ExpectedResultTest1 => 0; // TODO replace
    public static /*resultType*/int ExpectedResultTest2 => 0; // TODO replace
    
    private static bool Test2Started => ExpectedResultTest2 != 0;
    

    public static void Main(string[] args) {
        // TEST
        Solve(TestFileName, out var resultTest1, out var resultTest2);
        const string success = "✅"; const string fail = "❌";
        Console.WriteLine($"Test result 1: {(resultTest1 == ExpectedResultTest1 ? success : fail)} (result: {resultTest1}, expected: {ExpectedResultTest1})");
        if (Test2Started)
            Console.WriteLine($"Test result 2: {(resultTest2 == ExpectedResultTest2 ? success : fail)} (result: {resultTest2}, expected: {ExpectedResultTest2})");
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    

        // ACTUAL INPUT
        Solve(InputFileName, out var result1, out var result2);
        Console.WriteLine($"Result 1: {result1}");
        Console.WriteLine($"Result 2: {result2}");
    }

    public static void Solve(string inputFileName, out /*resultType*/int result1, out /*resultType*/int result2) {
        result1 = 0; 
        result2 = 0;

#if(LineByLineArray)
        var allLines = File.ReadAllLines(inputFileName).ToArray(); // .ToList();
        var width = allLines[0].Length;
        var height = allLines.Length; // .Count;
#elif(LineByLineList)
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        var width = allLines[0].Length;
        var height = allLines.Count; // .Length;
#else
        var allLines = File.ReadAllLines(inputFileName).ToArray(); // .ToList();
        var width = allLines[0].Length;
        var height = allLines.Length; // .Count;
#endif
        
        for (int y = 0; y < height; y++) {
            var line = allLines[y];
            for (int x = 0; x < width; x++) {
                var c = line[x];
                // TODO your code here..
            }
        }
        
        
    }
    
}