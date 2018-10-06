using System;
using System.Text;

namespace SimonP.ConsoleUtil
{
	public sealed class ConsolePromptGenerator
	{
		public bool PromptBool(string prompt) => PromptBool(prompt, true);

		public bool PromptBool(string prompt, bool defaultValue)
		{
			try
			{
				while (true)
				{
					Console.Write(prompt);
					Console.Write(" (");
					Console.Write(defaultValue ? "[y]" : "y");
					Console.Write('/');
					Console.Write(!defaultValue ? "[n]" : "n");
					Console.Write(") ");

					ConsoleKey key = Console.ReadKey().Key;

					switch (key)
					{
						case ConsoleKey.Y:
							return true;
						case ConsoleKey.N:
							return false;
						case ConsoleKey.Enter:
							return defaultValue;
					}

					Console.WriteLine();
				}
			}
			finally
			{
				Console.WriteLine();
			}
		}

		public string PromptPassword(string prompt)
		{
			bool oldVisible = Console.CursorVisible;
			Console.CursorVisible = true;
			Console.Write(prompt);

			var stringBuilder = new StringBuilder();

			try
			{
				while (true)
				{
					int currentLeft = Console.CursorLeft;
					int currentTop = Console.CursorTop;

					ConsoleKeyInfo keyInfo = Console.ReadKey();

					if (keyInfo.Key == ConsoleKey.Enter)
					{
						return stringBuilder.ToString();
					}
					else
					{
						Console.SetCursorPosition(currentLeft, currentTop);
						Console.Write(' ');
						Console.SetCursorPosition(currentLeft, currentTop);

						stringBuilder.Append(keyInfo.KeyChar);
					}
				}
			}
			finally
			{
				Console.CursorVisible = oldVisible;
				Console.WriteLine();
			}
		}

		public T Prompt<T>(string prompt) => Prompt(prompt, default(T));

		public T Prompt<T>(string prompt, T defaultValue)
		{
			bool oldVisible = Console.CursorVisible;
			Console.CursorVisible = true;

			try
			{
				while(true)
				{
					Console.Write(prompt);
					string input = Console.ReadLine();

					if (input == null)
						return defaultValue;

					try
					{
						return (T)Convert.ChangeType(input, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
					}
					catch (Exception x) when (x is InvalidCastException || x is FormatException || x is OverflowException)
					{
						Console.WriteLine(x.Message);
					}
				}
			}
			finally
			{
				Console.CursorVisible = oldVisible;
				Console.WriteLine();
			}
		}
	}
}
