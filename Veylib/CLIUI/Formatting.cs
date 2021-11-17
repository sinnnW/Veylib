using System;

namespace Veylib.CLIUI
{
    class Formatting
    {
        private static Formatting inst = null;
        public static Formatting GetInstance()
        {
            if (inst == null)
                inst = new Formatting();
            return inst;
        }

        public string CreateDivider()
        {
            return new string('=', (int)Math.Round(Console.BufferWidth / 1.25));
        }

        public string CreateDivider(string Title)
        {
            var str = CreateDivider();
            var half = str.Substring(0, (int)Math.Round((decimal)str.Length / 2));
            return $"{half} {Title} {half}";
        }

        public string Center(string Input)
        {
            return $"{new string(' ', (Console.BufferWidth / 2) - (Input.Length / 2))}{Input}";
        }

        public string Space(string Input, int Length)
        {
            return $"{Input}{new string(' ', (Length - Input.Length > 0 ? Length - Input.Length : 0))}";
        }
    }
}
