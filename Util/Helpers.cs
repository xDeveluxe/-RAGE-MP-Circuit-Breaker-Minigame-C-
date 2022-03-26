using System;

namespace CircuitBreaker
{
    public class Vector2
    {
        public static Vector2 Zero { get; } = new Vector2();
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2()
        {
            X = 0f;
            Y = 0f;
        }

        public Vector2(Vector2 source)
        {
            X = source.X;
            Y = source.Y;
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            Vector2 vector = new Vector2(a.X - b.X, a.Y - b.Y);
            return MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }
    }

    public class Helpers
    {
        private static readonly Random Rand = new Random();
        public static int GetRange(int firstNum, int secondNum)
        {
            return Rand.Next(firstNum, secondNum);
        }

        public static double GetRange()
        {
            return Rand.NextDouble();
        }

        public static bool HasStreamedTextureDictLoaded(string dictionary, bool requestifnot = true)
        {
            if (!RAGE.Game.Graphics.HasStreamedTextureDictLoaded(dictionary))
            {
                if (requestifnot && RequestStreamedTextureDictionaryRightNow(dictionary)) return true;
                return false;
            }
            return true;
        }

        public static bool RequestStreamedTextureDictionaryRightNow(string dictionary)
        {
            RAGE.Game.Graphics.RequestStreamedTextureDict(dictionary, false);
            while (!RAGE.Game.Graphics.HasStreamedTextureDictLoaded(dictionary)) RAGE.Game.Invoker.Wait(0);
            return true;
        }
    }
}
