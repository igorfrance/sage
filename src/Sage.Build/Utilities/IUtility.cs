namespace Sage.Build.Utilities
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	internal interface IUtility
	{
		string CommandName { get; }

		bool ParseArguments(string[] args);

		string GetUsage();

		void Run();
	}
}
