using System;
namespace Samurai
{
    public static class Logs
    {
        public static void PrintException(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintException(Exception e)
        {
            PrintException($"{e.GetType().Name}: {e.Message}");
        }

        public static void PrintImportantStep(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
