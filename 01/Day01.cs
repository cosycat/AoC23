namespace _01; 

public static class Day01 {

    public static void Main(string[] args) {
        var allLines = File.ReadAllLines("input.txt").ToList();
        // using var outp = new StreamWriter("output.txt");
        // outp.Write("solution");

        var total = 0;

        foreach (var line in allLines) {
            var a = -1;
            var b = -1;
            for (int i = 0; i < line.Length; i++) {
                var n = ParseNumber(line, i);
                if (n != -1) {
                    if (a == -1) a = n;
                    b = n;
                }
            }

            total += 10 * a + b;

        }

        Console.WriteLine($"calibration value: {total}");
        
    }
    
    private static readonly List<string> Digits = new() { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

    private static int ParseNumber(string line, int i) {
        if (line[i] is >= '0' and <= '9') return int.Parse($"{line[i]}");
        for (var digitIndex = 0; digitIndex < Digits.Count; digitIndex++) {
            var digit = Digits[digitIndex];
            if (line.Length - i < digit.Length) continue;
            var fits = true;
            for (int j = 0; j < digit.Length && fits; j++) {
                if (line[i + j] != digit[j])
                    fits = false;
            }

            if (fits) return digitIndex + 1;
        }

        return -1;
    }
}