using System;

class Program
{
    static void Main()
    {
        Console.Write("이름: ");
        string name = Console.ReadLine() ?? "";

        int age = ReadFirstInt("나이: ");

        Console.WriteLine($"안녕하세요 {name}님, 내년엔 {age + 1}살이에요.");
    }

    static int ReadFirstInt(string prompt)
    {
        const int MIN = 0;
        const int MAX = 150;
        const int MAX_TRIES = 3;

        for (int attempt = 1; attempt <= MAX_TRIES; attempt++)
        {
            Console.Write($"{prompt} (0~150, 종료: q) ");
            string line = (Console.ReadLine() ?? "").Trim();

            if (line.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("입력을 종료합니다.");
                Environment.Exit(0);
            }

            // 공백/탭 기준으로 분리 후 첫 토큰만 사용
            string[] parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && int.TryParse(parts[0], out int value))
            {
                if (value >= MIN && value <= MAX)
                    return value;

                Console.WriteLine($"범위를 벗어났습니다. {MIN}~{MAX} 사이로 입력하세요.");
            }
            else
            {
                Console.WriteLine("숫자를 올바르게 입력하세요. 예: 23");
            }

            Console.WriteLine($"남은 시도: {MAX_TRIES - attempt}");
        }

        Console.WriteLine("시도 횟수를 초과했습니다. 기본값 0을 사용합니다.");
        return 0;
    }
}