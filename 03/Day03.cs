namespace _03; 

public class Day03 {

    public static void Main(string[] args) {
        var allLines = File.ReadAllLines("input.txt").ToList();
        
        var width = allLines[0].Length;
        var height = allLines.Count;
        
        var numbersField = new Number?[height, width];
        var allNumbers = new List<Number>();

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var c = allLines[y][x];
                if (c is >= '0' and <= '9') {
                    if (x > 0 && numbersField[y, x - 1] != null)
                        numbersField[y, x] = numbersField[y, x - 1];
                    else {
                        numbersField[y, x] = new Number();
                        allNumbers.Add(numbersField[y, x]!);
                    }

                    numbersField[y,x]!.AddDigit(c);
                }
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (allLines[y][x] == '.' || (allLines[y][x] >= '0' && allLines[y][x] <= '9')) {
                    continue;
                }

                // Console.WriteLine($"Found symbol {allLines[y][x]} at {x},{y}");
                
                // Is Symbol
                for (int y1 = Math.Max(0, y-1); y1 < Math.Min(y+2, height); y1++) {
                    for (int x1 = Math.Max(0, x-1); x1 < Math.Min(x+2, width); x1++) {
                        if (numbersField[y1, x1] != null) {
                            // Console.WriteLine($"Found number {numbersField[y1, x1]!.Value} at {x1},{y1}, for symbol {allLines[y][x]} at {x},{y}");
                            numbersField[y1, x1]!.SetValid();
                        }
                    }
                }
            }
        }

        Console.WriteLine($"All numbers: {allNumbers.Sum(n => n.Value)} ({allNumbers.Count})");
        Console.WriteLine(
            $"Valid numbers: {allNumbers.Where(n => n.Valid).Sum(n => n.Value)} ({allNumbers.Count(n => n.Valid)})");
        
        var solution1 = allNumbers.Where(n => n.Valid).Sum(n => n.Value);
        Console.WriteLine($"Solution 1: {solution1}");


    }

    private class Number {
        public int Value { get; private set; } = 0;
        public bool Valid { get; private set; } = false;
        public void AddDigit(char c) {
            Value = Value * 10 + (c - '0');
        }
        public void SetValid() {
            Valid = true;
        }
    }
    
}

