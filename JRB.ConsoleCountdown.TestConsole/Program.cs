using System;

using JRB.ConsoleCountdown;

namespace JRB.ConsoleCountdown.TestConsole
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to start the timer.");
            Console.ReadKey(true);

            Console.Write("Timer:  ");
            ConsoleCountdown countdown = new ConsoleCountdown();
            countdown.KeyPressed += Countdown_KeyPressed;
            CountdownResult? result = countdown.Prompt(30);
            Console.WriteLine();

            if (result == null)
                throw new InvalidOperationException("The result should never be null.");

            switch (result.PromptResult)
            {
                case PromptResult.TimerExpired:
                    Console.WriteLine("The timer expired.");
                    break;
                case PromptResult.UserStopped:
                    Console.WriteLine(result.Message);
                    if (result.KeyPressed.HasValue)
                    {
                        Console.WriteLine(result.KeyPressed.Value.KeyChar);
                    }
                    else
                    {
                        Console.WriteLine("The KeyPressed property is still null.");
                    }
                    break;
                case PromptResult.UserPaused:

                    break;
                case PromptResult.UserRestarted:
                    break;
                case PromptResult.Unknown:
                default:
                    Console.WriteLine("This shouldn't have happened.");
                    break;
            }

            Console.WriteLine();
        }

        private static void Countdown_KeyPressed(object? sender, KeyPressedEventArgs e)
        {
            if (e.KeyInfo.KeyChar == 'c' || e.KeyInfo.KeyChar == 'C')
            {
                e.TimerAction = TimerAction.Stop;
                e.Message = "User stopped.";
            }
            else if (e.KeyInfo.KeyChar == 'p' || e.KeyInfo.KeyChar == 'P')
            {
                e.TimerAction = TimerAction.Pause;
                e.Message = "Paused";
            }
            else if (e.KeyInfo.KeyChar == 'r' || e.KeyInfo.KeyChar == 'R')
            {
                e.TimerAction = TimerAction.Restart;
            }
        }
    }
}
