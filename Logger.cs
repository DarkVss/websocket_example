#nullable enable
using System;

namespace WebSocket_example{
    /// <summary>
    ///     Console.WriteLine colored
    /// </summary>
    /// <example>
    /// Log.Success($"Test log message");
    /// Log.Info($"Test log message");
    /// Log.Warning($"Test log message");
    /// Log.Error($"Test log message");
    /// Log.Default($"Test log message");
    /// </example>
    public struct Logger{
        public static void Success(string message){
            Write(message, "[S]", ConsoleColor.Green);
        }

        public static void Info(string message){
            Write(message, "[I]", ConsoleColor.Blue);
        }

        public static void Warning(string message){
            Write(message, "[W]", ConsoleColor.Yellow);
        }

        public static void Error(string message){
            Write(message, "[E]", ConsoleColor.Red);
        }

        public static void Default(string message){
            Write(message, "[-]", ConsoleColor.Black);
        }

        public static void Write(string message, string mark, ConsoleColor? color = null){
            Console.BackgroundColor = ConsoleColor.Black;
            color ??= Console.BackgroundColor;

            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{DateTime.Now} ");

            Console.ForegroundColor = Console.BackgroundColor;
            Console.BackgroundColor = (ConsoleColor) color;
            Console.Write(mark);


            Console.ResetColor();
            Console.WriteLine(" " + message);
        }
    }
}