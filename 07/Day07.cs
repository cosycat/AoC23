using System.Diagnostics;
using System.Text.RegularExpressions;

namespace d07y2023; 

public static class Day07 {
    private const long ExpectedResultTest1 = 6440;
    private const long ExpectedResultTest2 = 5905;
    private const string InputFileName = "inputDay07.txt";
    private const string TestFileName = "testInputDay07.txt";
    private static bool Test2Started => ExpectedResultTest2 != 0;
    
    private const long ActualResult1 = 251029473; // For ensuring it stays correct, once the actual result is known
    private const long ActualResult2 = 0; // For ensuring it stays correct, once the actual result is known
    
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
    
    private static void PrintResult(long result, long expected, int index, bool isTest = false) {
        Console.WriteLine($"{(isTest ? "Test " : "")}Result {index}: {result} {(expected == 0 ? "" : expected == result ? Success : Fail + $"(expected: {expected})")} ");
    }
    
    [Conditional("DEBUG")]
    private static void TestRun() {
        Solve(TestFileName, out var resultTest1, out var resultTest2);
        PrintResult(resultTest1, ExpectedResultTest1, 1, true);
        if (Test2Started)
            PrintResult(resultTest2, ExpectedResultTest2, 2, true);
        Console.WriteLine();

        Debug.Assert(ExpectedResultTest1 != 0, "No expected result for test 1 set!");
        Debug.Assert(ExpectedResultTest1 == resultTest1, "Test 1 failed!");
        Debug.Assert(!Test2Started || ExpectedResultTest2 == resultTest2, "Test 2 failed!");
    }

    private static void Solve(string inputFileName, out long result1, out long result2) {
        result1 = 0; 
        result2 = 0;
        
        var allLines = File.ReadAllLines(inputFileName).ToList(); // .ToArray();
        Debug.Assert(allLines.Count > 0, $"Input file {inputFileName} is empty!");
        
        List<Hand> hands = new();
        for (int i = 0; i < allLines.Count; i++) {
            var line = allLines[i].Split(" ");
            var hand = new Hand(line[0], int.Parse(line[1]));
            hands.Add(hand);
        }
        hands.Sort();
        for (int i = 0; i < hands.Count; i++) {
            result1 += (i + 1) * hands[i].Value;
            hands[i].UpdateRating(jokerVersion: true); // for part 2
        }
        hands.Sort();
        for (int i = 0; i < hands.Count; i++) {
            result2 += (i + 1) * hands[i].Value;
        }
        
    }
    
}

class Hand : IComparable<Hand> {
    public int Value { get; }

    private int[] _cards = new int[5];

    private HandRating _rating;
    
    public Hand(string cardInput, int value) {
        Value = value;
        for (int i = 0; i < 5; i++) {
            _cards[i] = ParseCard(cardInput[i]);
        }
        UpdateRating(false);
    }

    public void UpdateRating(bool jokerVersion = false) {
        _rating = GetRating(jokerVersion);
    }

    private HandRating GetRating(bool jokerVersion = false) {
        if (jokerVersion) {
            for (int i = 0; i < 5; i++) {
                if (_cards[i] == 11) _cards[i] = 1;
            }
        }
        
        // Regular rules
        if (!jokerVersion || _cards.All(c => c != 1)) {
            if (_cards.Distinct().Count() == 1) {
                return HandRating.FiveOfAKind;
            }

            var groups = _cards.GroupBy(c => c).ToList();
            if (groups.Count == 2) {
                return groups.Any(g => g.Count() == 4) ? HandRating.FourOfAKind : HandRating.FullHouse;
            }

            if (groups.Count == 3) {
                return groups.Any(g => g.Count() == 3) ? HandRating.ThreeOfAKind : HandRating.TwoPairs;
            }

            if (groups.Count == 4) {
                return HandRating.Pair;
            }

            return HandRating.HighCard;
        }
        
        Debug.Assert(_cards.Any(c => c == 1), "No joker found!");
        // Joker rules And at least one joker
        var groups2 = _cards.GroupBy(c => c).ToList();

        if (groups2.Count is 1 or 2) {
            return HandRating.FiveOfAKind; // All Jokers or jokers and one other type
        }

        if (groups2.Count == 3) {
            if (groups2.First(g => g.Key == 1).Count() == 1) {
                if (groups2.Any(g => g.Count() == 2)) {
                    return HandRating.FullHouse;
                }
                else {
                    return HandRating.FourOfAKind;
                }
            }
            else {
                return HandRating.FourOfAKind;
            }
        }
        
        if (groups2.Count == 4) {
            return HandRating.ThreeOfAKind;
        }
        
        return HandRating.Pair;

    }


    private int ParseCard(char c) {
        return c switch {
            'A' => 14,
            'K' => 13,
            'Q' => 12,
            'J' => 11,
            'T' => 10,
            _ => c - '0'
        };
    }

    public int CompareTo(Hand? other) {
        if (other == null) return 1;
        if (_rating != other._rating) return _rating.CompareTo(other._rating);
        for (int i = 0; i < 5; i++) {
            if (_cards[i] != other._cards[i]) return _cards[i].CompareTo(other._cards[i]);
        }
        return 0;
    }
}

internal enum HandRating {
    HighCard = 1,
    Pair,
    TwoPairs,
    ThreeOfAKind,
    FullHouse,
    FourOfAKind,
    FiveOfAKind,
}

