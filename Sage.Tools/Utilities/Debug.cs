namespace Sage.Tools.Utilities
{
	using System;
	using System.Text;
	using System.Threading;

	internal class Debug : IUtility
	{
		private const int TimerMilliseconds = 500;

		public string CommandName
		{
			get
			{
				return "debug";
			}
		}

		public bool ParseArguments(string[] args)
		{
			return true;
		}

		public string GetUsage()
		{
			StringBuilder result = new StringBuilder();
			result.AppendLine("Starts a continuous loop on a .5 second timer, allowing an external debugger to attach to it.");
			result.AppendLine("Once started, the program can be stopped by pressing SPACE.\n");
			result.AppendFormat("Usage: {0} {1}", Program.Name, this.CommandName);

			return result.ToString();
		}

		public void Run()
		{
			ShowInstructions();
			while (true)
			{
				if (Console.KeyAvailable)
				{
					ConsoleKeyInfo info = Console.ReadKey();
					if (info.Key == ConsoleKey.Spacebar)
						break;
				}

				Thread.Sleep(TimeSpan.FromMilliseconds(Debug.TimerMilliseconds));
			}
		}

		private static void ShowInstructions()
		{
			var foregroundColor = Console.ForegroundColor;
			var backgroundColor = Console.BackgroundColor;

			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.DarkGray;

			Console.WriteLine();
			Console.Write("Press space to terminate.");

			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;

			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
