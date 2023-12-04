using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d04y2023; 

public static class Day04 {
    private const int ExpectedResultTest1 = 13;
    private const int ExpectedResultTest2 = 30;
    private const string InputFileName = "inputDay04.txt";
    private const string TestFileName = "testInputDay04.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const int ActualResult1 = 21485;
    private const int ActualResult2 = 11024379;
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
    
    private static void PrintResult(int result, int expected, int index) {
        Console.WriteLine($"Result {index}: {result} {(expected == 0 ? "" : expected == result ? Success : Fail + $"(expected: {expected})")} ");
    }
    
    [Conditional("DEBUG")]
    private static void TestRun() {
        Solve(TestFileName, out var resultTest1, out var resultTest2);
        Console.WriteLine(
            $"Test result 1: {(resultTest1 == ExpectedResultTest1 ? Success : Fail)} (result: {resultTest1}, expected: {ExpectedResultTest1})");
        if (Test2Started)
            Console.WriteLine(
                $"Test result 2: {(resultTest2 == ExpectedResultTest2 ? Success : Fail)} (result: {resultTest2}, expected: {ExpectedResultTest2})");
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out int result1, out int result2) {
        result1 = 0; 
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        var width = allLines[0].Length;
        var height = allLines.Count;
        
        List<(int matchCount, int cardValue)> allCards = new();

        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i];

            var bothNumbers = line.Split(":")[1].Split("|");
            Debug.Assert(bothNumbers.Length == 2, $"Line {i} does not have 2 numbers");

            const string numbersPattern = @"(?:\s*(?'Number'\d+))+";

            var winningNumbers = Regex.Match(bothNumbers[0], numbersPattern).Groups["Number"].Captures.Select(c => int.Parse(c.Value)).ToList();
            var ourNumbers = Regex.Match(bothNumbers[1], numbersPattern).Groups["Number"].Captures.Select(c => int.Parse(c.Value)).ToList();
            
            Debug.Assert(winningNumbers.Distinct().Count() == winningNumbers.Count, $"Line {i} has duplicate winning numbers");
            Debug.Assert(ourNumbers.Distinct().Count() == ourNumbers.Count, $"Line {i} has duplicate our numbers");

            // Console.WriteLine($"Line {i}: {string.Join(", ", winningNumbers)} | {string.Join(", ", ourNumbers)}");
            
            var matches = ourNumbers.Intersect(winningNumbers).ToList();
            var matchesCount = matches.Count;
            // Console.WriteLine($"Line {i}: {matchesCount} matches: {string.Join(", ", matches)}");

            var cardValue = 0;
            if (matchesCount > 0) {
                cardValue = 1;
                for (int j = 0; j < matchesCount - 1; j++) {
                    cardValue *= 2;
                }

                result1 += cardValue;
            }

            allCards.Add((matchesCount, cardValue));
        }

        // faster (about 1ms)
        var cardCopyCount = new int[allCards.Count];
        for (int i = 0; i < cardCopyCount.Length; i++) {
            var matches = allCards[i].matchCount;
            result2 += cardCopyCount[i] + 1; // add one for the original card
            for (int j = 0; j < matches; j++) {
                cardCopyCount[i + j + 1] += cardCopyCount[i] + 1; // add one for the original card
            }
        }

        // slower (about 180ms)
        // List<int> cardInstancesProcessed = new();
        // Stack<int> cardInstancesToProcess = new();
        // for (int i = 0; i < allCards.Count; i++) {
        //     cardInstancesToProcess.Push(i);
        // }
        //
        // while (cardInstancesToProcess.Count > 0) {
        //     var cardIndex = cardInstancesToProcess.Pop();
        //     // Console.WriteLine($"Processing card {cardIndex}");
        //     cardInstancesProcessed.Add(cardIndex);
        //     var card = allCards[cardIndex];
        //     result2++;
        //     for (int i = 0; i < card.matchCount; i++) {
        //         var nextCardIndex = cardIndex + i + 1;
        //         Debug.Assert(allCards.Count > nextCardIndex, $"Card {nextCardIndex} does not exist");
        //         cardInstancesToProcess.Push(nextCardIndex);
        //         // Console.WriteLine($"  Adding card {nextCardIndex}");
        //     }
        // }
        //
        // // Console.WriteLine($"Cards {allCards.Count} (matches/value): {string.Join(", ", allCards.Select(c => $"{c.matchCount}/{c.cardValue}"))}");
        //
        // Debug.Assert(cardInstancesProcessed.Count == result2);

    }
    
}
