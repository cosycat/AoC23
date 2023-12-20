using System.Diagnostics;
using System.Text.RegularExpressions;

namespace dxxyCurrYear;

public static partial class Dayxx {
    private const string InputFileName = "inputDayxx.txt";
    private static bool Test2Started => Tests.Any(t => t.expectedResult2 != null);

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
    
    private static void PrintResult(/*resultType*/int result, /*resultType*/int expected, int index, bool isTest = false) {
        Console.WriteLine($"{(isTest ? "Test " : "")}Result {index}: {result} {(expected == 0 ? "" : expected == result ? Success : Fail + $"(expected: {expected})")} ");
    }
    
    [Conditional("DEBUG")]
    private static void TestRun() {
        var failed = 0;
        Console.WriteLine("Running tests...");
        Console.WriteLine("--------------------------------------------------");
        foreach (var (testFileName, expectedResultTest1, expectedResultTest2) in Tests) {
            Console.WriteLine($"Testing file: {testFileName}");
            
            Solve(testFileName, out var resultTest1, out var resultTest2);
            if (expectedResultTest1 is not (null or 0)) {
                PrintResult(resultTest1, expectedResultTest1.Value, 1, true);
            }
            else {
                Console.WriteLine($"No expected result for test 1 set in {testFileName}!");
                failed++;
            }

            if (Test2Started)
                PrintResult(resultTest2, expectedResultTest2!.Value, 2, true);
            
            if (expectedResultTest1 != resultTest1) {
                Console.WriteLine($"Test 1 failed in {testFileName}!");
                failed++;
            }
            else if (Test2Started && expectedResultTest2 != resultTest2) {
                Console.WriteLine($"Test 2 failed in {testFileName}!");
                failed++;
            }
            Console.WriteLine("--------------------------------------------------");
        }
        
        Debug.Assert(ContinueIfTestsFail || failed == 0, $"Tests passed: {Tests.Count - failed}/{Tests.Count}");
        
        Console.WriteLine();
    }
}
