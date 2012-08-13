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
	/// Defines the way in which to internationalize an <see cref="XmlResource"/>.
	/// </summary>
	public enum InternationalizeType
	{
		/// <summary>
		/// Indicates the standard processing mode, where values are simply resolved and translated.
		/// </summary>
		Translate = 0,

		/// <summary>
		/// Indicates the diagnostic processing mode, where additional debugging information is emitted with each processed value.
		/// </summary>
		Diagnose = 1,
	}
}
