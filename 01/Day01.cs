namespace _01; 

public class Day01 {

    public static void Main(string[] args) {
        var allLines = File.ReadAllLines("input.txt").ToList();
        // using var outp = new StreamWriter("output.txt");
        // outp.Write("solution");

        var total = 0;

        foreach (var line in allLines) {
            var a = -1;
            var b = -1;
            for (int i = 0; i < line.Length; i++) {
                var c = line[i];
                if (c is >= '0' and <= '9') {
                    var n = int.Parse($"{c}");
                    if (a == -1) a = n;
                    b = n;
                }
            }

            total += 10 * a + b;

        }

        Console.WriteLine($"calibration value: {total}");
        
    }
    
}