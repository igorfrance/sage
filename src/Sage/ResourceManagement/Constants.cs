namespace Sage.ResourceManagement
{
	using Sage.Configuration;

	/// <summary>
	/// Defines the way in which the supplied path template should be resolved.
	/// </summary>
	public enum PathResolveType
	{
		/// <summary>
		/// Indicates that resolving should leave the {locale} placeholder unresolved.
		/// </summary>
		NoLocalize = 0,

		/// <summary>
		/// Indicates that resolving should replace the {locale} placeholder with the specified <c>locale</c>.
		/// </summary>
		Localize = 1,

		/// <summary>
		/// Indicates that resolving should replace the {locale} with the closest match to the specified <c>locale</c>, as defined with
		/// <see cref="LocaleInfo.DictionaryNames"/>.
		/// </summary>
		LocalizeToExisting = 2,
	}

	/// <summary>
	/// Defines the way in which to globalize an <see cref="XmlResource"/>.
	/// </summary>
	public enum GlobalizeType
	{
		/// <summary>
		/// Indicates the standard processinbg mode, where values are simply resolved and translated.
		/// </summary>
		Translate = 0,

		/// <summary>
		/// Indicates the diagnostic processinbg mode, where additional debugging information is emitted with each processed value.
		/// </summary>
		Diagnose = 1,
	}
}
