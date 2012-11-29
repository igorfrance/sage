/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sage.Tools.Utilities
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;

	internal class FileMonitor : IUtility
	{
		private static readonly ConsoleColor foregroundColor = Console.ForegroundColor;
		private static readonly ConsoleColor backgroundColor = Console.BackgroundColor;

		private static int refreshTimeout = 50;

		private static DateTime lastChecked;
		private static string filePath;
		private static int lastIndex;
		private static long lastLength;

		public string CommandName
		{
			get
			{
				return "monitor";
			}
		}

		public bool ParseArguments(string[] args)
		{
			foreach (string arg in args)
			{
				if (arg.StartsWith("-path:"))
				{
					filePath = arg.Substring(6).Trim('\'', '"');
				}

				if (arg.StartsWith("-i:"))
				{
					int interval;

					if (int.TryParse(arg.Substring(3), out interval))
					{
						interval = Math.Min(50, interval);
						interval = Math.Max(30000, interval);

						refreshTimeout = interval;
					}
				}
			}

			if (!string.IsNullOrEmpty(filePath))
			{
				if (!Path.IsPathRooted(filePath))
					filePath = Path.Combine(Program.ApplicationPath, filePath);

				return true;
			}

			return false;
		}

		public string GetUsage()
		{
			var result = new StringBuilder();
			result.AppendLine("Monitors the specified file for changes and writes its contents to console.\n");
			result.AppendFormat("Usage: {0} {1} -path:<path> [-i:<interval>]\n", Program.Name, this.CommandName);
			result.AppendLine("  -path:<path>     The path to the file to monitor.");
			result.AppendLine("     -i:<interval> The refresh interval (in milliseconds). Default is 250.");
			result.AppendLine("                   Minimum is 50 and maximum is 30000 (30 seconds).");

			return result.ToString();
		}

		public void Run()
		{
			int loopCounter = 0;
			bool keepLooping = true;

			while (keepLooping)
			{
				try
				{
					if (Console.KeyAvailable)
					{
						ConsoleKeyInfo info = Console.ReadKey();
						if (info.Key == ConsoleKey.Spacebar)
							break;

						if (info.Key == ConsoleKey.C)
						{
							Console.Clear();
							ShowInstructions();
						}
					}

					long fileLength = new FileInfo(filePath).Length;
					if (fileLength != lastLength || File.GetLastWriteTime(filePath) > lastChecked)
					{
						ShowNewLines();
						loopCounter = 0;
					}

					if (++loopCounter == 4)
						ShowInstructions();
				}
				catch (Exception e)
				{
					Console.ForegroundColor = ConsoleColor.White;
					Console.BackgroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("EXIT: " + e.Message);
					Console.ForegroundColor = foregroundColor;
					Console.BackgroundColor = backgroundColor;
					keepLooping = false;
				}

				Thread.Sleep(refreshTimeout);
			}
		}

		private static void ShowNewLines()
		{
			string[] lines;
			using (FileStream s = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				var contents = new byte[s.Length];
				s.Read(contents, 0, (int) s.Length);
				s.Close();

				string text = Encoding.UTF8.GetString(contents);
				lines = text.Replace("\r", string.Empty).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			}

			if (lines.Length < lastIndex)
				lastIndex = 0;

			for (int i = lastIndex; i < lines.Length; i++, lastIndex++)
			{
				var line = new Line(lines[i]);
				Console.ForegroundColor = line.Color ?? foregroundColor;
				Console.BackgroundColor = line.BgColor ?? backgroundColor;
				Console.WriteLine(line.Text);
			}

			lastChecked = DateTime.Now;
			lastLength = new FileInfo(filePath).Length;
		}

		private static void ShowInstructions()
		{
			Console.ForegroundColor = ConsoleColor.Black;
			Console.BackgroundColor = ConsoleColor.DarkGray;

			Console.WriteLine();
			Console.Write("Press space to terminate, C to clear");

			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;

			Console.WriteLine();
			Console.WriteLine();
		}

		private class Line
		{
			private static readonly Regex lineCheck = new Regex(@"\b(WARN|ERROR|INFO|FATAL)\b", RegexOptions.Compiled);

			public Line(string text)
			{
				this.Text = text;

				Match match;
				if (!(match = lineCheck.Match(text)).Success)
					return;

				switch (match.Groups[1].Value)
				{
					case "INFO":
						this.Color = ConsoleColor.DarkGreen;
						break;

					case "WARN":
						this.Color = ConsoleColor.DarkYellow;
						break;

					case "ERROR":
						this.Color = ConsoleColor.Red;
						break;

					case "FATAL":
						this.Color = ConsoleColor.White;
						this.BgColor = ConsoleColor.Red;
						break;
				}
			}

			public string Text { get; private set; }

			public ConsoleColor? Color { get; private set; }

			public ConsoleColor? BgColor { get; private set; }
		}
	}
}
