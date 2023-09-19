using System;

namespace Demo
{
    class Program
    {
        static void Main()
        {
            var expression = "0-30/5 * * * * *";
            Console.WriteLine(expression);
            var timer = new CronTimer(expression, "Asia/Hong_Kong", includingSeconds: true);
            timer.OnOccurence += (s, ea) => Console.WriteLine($"{ea.At:T} - {DateTime.Now}");
            timer.Start();

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
            }

            timer.Stop();
        }
    }
}
