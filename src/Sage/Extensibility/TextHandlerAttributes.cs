namespace Sage.Extensibility
{
	using System;
	using System.Collections.Generic;

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class TextHandlerAttribute : Attribute
	{
		private TextHandlerAttribute()
		{
			this.Variables = new List<string>();
		}

		public TextHandlerAttribute(string variable)
			: this()
		{
			this.Variables.Add(variable);
		}

		public TextHandlerAttribute(params string[] variables)
			: this()
		{
			foreach (string var in variables)
			{
				if (!this.Variables.Contains(var))
					this.Variables.Add(var);
			}
		}

		public IList<string> Variables { get; private set; }
	}
}
