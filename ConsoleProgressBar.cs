using System;

namespace AGDConverter
{
    public class ConsoleProgressBar
    {
        private readonly int _barLength;
        private readonly char _filledChar;
        private readonly char _emptyChar;
        private int _lastPercentage = -1;

        public ConsoleProgressBar(int barLength = 50, char filledChar = '█', char emptyChar = '░')
        {
            _barLength = barLength;
            _filledChar = filledChar;
            _emptyChar = emptyChar;
        }

        public void Update(int current, int total, string? message = null)
        {
            if (total <= 0) return;

            int percentage = (int)((double)current / total * 100);
            
            // Only update if percentage changed to avoid flickering
            if (percentage == _lastPercentage && string.IsNullOrEmpty(message))
                return;

            _lastPercentage = percentage;

            int filledLength = (int)((double)current / total * _barLength);
            int emptyLength = _barLength - filledLength;

            string filled = new string(_filledChar, filledLength);
            string empty = new string(_emptyChar, emptyLength);

            // Clear the current line and move cursor to beginning
            Console.Write("\r");
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.Write("\r");

            // Write progress bar
            Console.Write($"[{filled}{empty}] {percentage,3}% ({current}/{total})");

            if (!string.IsNullOrEmpty(message))
            {
                Console.Write($" - {message}");
            }
        }

        public void Complete(string? message = null)
        {
            // Clear the line and show completion
            Console.Write("\r");
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.Write("\r");
            
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"✓ {message}");
            }
            else
            {
                Console.WriteLine("✓ Complete");
            }
        }

        public void Clear()
        {
            Console.Write("\r");
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.Write("\r");
        }
    }
}
