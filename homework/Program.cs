using System;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length != 1 || args[0] != "TEST")
        {
            Console.WriteLine("실행 불가: 인자 TEST가 필요합니다.");
            Console.WriteLine("사용법: dotnet run -- TEST");
            return 1;
        }

        Console.WriteLine("키를 누르세요 (a/A, b/B). ESC 누르면 종료");
        while (true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape:
                    Console.WriteLine("종료합니다.");
                    return 0;
                case ConsoleKey.A:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        Console.WriteLine("대문자 A를 입력하였습니다");
                    }
                    else
                    {
                        Console.WriteLine("소문자 a를 입력하였습니다");
                    }
                    break;
                case ConsoleKey.B:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        Console.WriteLine("대문자 B를 입력하였습니다");
                    }
                    else
                    {
                        Console.WriteLine("소문자 b를 입력하였습니다");
                    }
                    break;
                default:
                    Console.WriteLine("다른 키를 입력하였습니다");
                    break;
            }
        }
    }
}