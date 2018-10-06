using System;

namespace SimonP.ConsoleUtil
{
	public sealed class ConsoleProgressBar
	{
		private int _consoleLeft, _consoleTop; // location of the progress bar in the console

		public char ProgressCharacter { get; set; } = '=';
		public int BarSize { get; set; } = 25;
		public ConsoleColor CompleteColor { get; set; } = ConsoleColor.Green;
		public ConsoleColor RemainingColor { get; set; } = ConsoleColor.DarkGreen;

		public ConsoleProgressBar(string leadingString)
		{
			if (leadingString == null)
				throw new ArgumentNullException(nameof(leadingString));

			Console.Write(leadingString);

			_consoleLeft = Console.CursorLeft;
			_consoleTop = Console.CursorTop;

			Console.WriteLine();
		}
		
		public void Draw(double completeRatio)
		{
			if (completeRatio < 0.0 - Double.Epsilon || completeRatio > 1.0 + Double.Epsilon)
				throw new ArgumentOutOfRangeException(nameof(completeRatio));

			bool oldVisible = Console.CursorVisible;
			int oldLeft = Console.CursorLeft;
			int oldTop = Console.CursorTop;

			try
			{
				Console.CursorVisible = false;
				Console.SetCursorPosition(_consoleLeft, _consoleTop);
				Console.Write('[');

				int numComplete = (int)Math.Floor(completeRatio * BarSize);

				Console.ForegroundColor = CompleteColor;
				Console.Write(new string(ProgressCharacter, numComplete));

				Console.ForegroundColor = RemainingColor;
				Console.Write(new string(ProgressCharacter, BarSize - numComplete));

				Console.ResetColor();
				Console.Write(']');

				Console.Write($" {completeRatio * 100:F2}%");
			}
			finally
			{
				Console.CursorVisible = oldVisible;
				Console.SetCursorPosition(oldLeft, oldTop);
			}
		}
	}
}
