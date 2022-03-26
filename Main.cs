using System;
using RAGE;

namespace Client
{
    public class Main : Events.Script
    {
        public Main()
        {
            Events.Add("CircuitBreakerStart", CircuitBreakerStart);

            Events.Add("CircuitBreakerWIN", CircuitBreakerWin);
            Events.Add("CircuitBreakerLOSE", CircuitBreakerLose);
        }

        private static void CircuitBreakerStart(object[] args)
        {
            if (args == null || args.Length != 3) return;
            int lives = Convert.ToInt32(args[0]);
            int difficulty = Convert.ToInt32(args[1]);
            int countOfLevels = Convert.ToInt32(args[2]);
            CircuitBreaker.CircuitBreaker.StartMinigame(lives, difficulty, countOfLevels);
        }

        private static void CircuitBreakerWin(object[] args)
        {
            // Do whatever you want
        }

        private static void CircuitBreakerLose(object[] args)
        {
            // Do whatever you want
        }

    }
}
