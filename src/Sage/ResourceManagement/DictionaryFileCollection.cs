namespace Sage.ResourceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Sage.Configuration;

	/// <summary>
	/// Combines the dictionary files and variables document in use for internationalization of a single category, .
	/// </summary>
	public class DictionaryFileCollection
	{
		private readonly SageContext context;

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryFileCollection"/>, using the specified context.
		/// </summary>
		/// <param name="context">The <see cref="SageContext"/> under which this <see cref="DictionaryFileCollection"/> 
		/// is being created and used.</param>
		public DictionaryFileCollection(SageContext context)
		{
			this.context = context;

			CategoryInfo info = context.Config.Categories[this.Category];
			this.Locales = new List<string>(info.Locales);

			this.Refresh();
		}

		/// <summary>
		/// Gets the category of this <see cref="DictionaryFileCollection"/>.
		/// </summary>
		public string Category
		{
			get
			{
				return context.Category;
			}
		}

		/// <summary>
		/// Gets the list of locales that this collection contains.
		/// </summary>
		/// <value>The locales.</value>
		public List<string> Locales
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the latest modified date of all objects in this collection.
		/// </summary>
		public DateTime DateLastModified
		{
			get
			{
				DateTime maxDate = DateTime.MinValue;
				var keys = this.Dictionaries.Keys.ToList();
				foreach (string key in keys)
				{
					if (this.Dictionaries[key].DateLastModified > maxDate)
						maxDate = this.Dictionaries[key].DateLastModified;
				}

				return maxDate;
			}
		}

		/// <summary>
		/// Gets the dictionary of localization files to use for translation.
		/// </summary>
		public Dictionary<string, DictionaryFile> Dictionaries
		{
			get;
			private set;
		}

		/// <summary>
		/// (Re) loads the constituent translation dictionaries.
		/// </summary>
		public void Refresh()
		{
			lock (context)
			{
				lock (context)
				{
					//// RESET DICTIONARIES
					this.Dictionaries = new Dictionary<string, DictionaryFile>();
					foreach (string locale in this.Locales)
						this.Dictionaries.Add(locale, new DictionaryFile(context, locale));
				}
			}
		}
	}
}
