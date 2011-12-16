namespace Sage.ResourceManagement
{
	using System.Runtime.CompilerServices;

	using Sage.Controllers;

	/// <summary>
	/// Resource Management namespace handles all localization and internationalization of resources.
	/// </summary>
	/// <remarks>
	/// <h5>Internationalization</h5>
	/// Many of the resource that the application are internationalized by either being: 
	/// <list type="number">
	/// <item><description>
	/// Localized (a custom version of a file is made for a single locale or a group of locales).
	/// </description></item>
	/// <item><description>
	/// Globalized (the text context of an XML file is translated to various locales that a category uses).
	/// </description></item>
	/// <item><description>
	/// Both.
	/// </description></item>
	/// </list>
	/// <h6>Globalization</h6>
	/// <para>
	/// Globalization, or translation of resources is a relatively straightforward process where a single XML file with
	/// various 'phrase' elements is processed using different language dictionaries to produce translated languages versions
	/// of the same source file. Globalization is done on XML files only. Each language configured for a site/category will
	/// get its own version of the file from a single template file that contains text placeholders rather than the actual
	/// language content.
	/// </para>
	/// <h6>Localization</h6>
	/// <para>
	/// Localization, or customization of resources based on the target locale, is achieved by having multiple localized 
	/// versions of the same file. For instance, if there is a style-sheet file <c>typography.css</c> that should be 
	/// customized for Chinese, a copy of it should be made for china with the suffix in the form <c>-{locale}</c> added to
	/// it, and the original file should get the <c>-default</c> suffix added to it to indicate that it applies to all
	/// regions without a more specific localized version. Therefore, in this example:
	/// </para>
	/// <code>typography.css</code>
	/// <para>is copied to <c>typography-cn.css</c> and renamed to <c>typography-default.css</c>, so we now have 
	/// two files:</para>
	/// <code>
	/// typography-default.css
	/// typography-cn.css
	/// </code>
	/// With this setup in place, when a request for the file <c>typography.css</c> comes through the resource management
	/// system (typically through the <see cref="FileProxyController"/>, the current locale is taken into account, and the 
	/// most appropriate version of the file is chosen automatically. In this example, if the current locale is Chinese, 
	/// the <c>typography-cn.css</c> file will be served, and in all other case <c>typography-default.css</c> will be 
	/// returned.
	/// <h6>Localization + globalization</h6>
	/// <para>
	/// Since the localization method described above can be applied on any desired resource, when dealing with XML 
	/// resources that already get globalized automatically, we can have a combination of these two internationalization
	/// methods.
	/// </para>
	/// <para>
	/// What happens in this case is that, just like with globalization, a version of the source file will be translated
	/// into each of the locales configured to the file's category. The only difference is that, if a more specific version
	/// of the source file exists (using the naming method described in the localization section earlier), it will be that
	/// version that gets translated.
	/// </para>
	/// </remarks>
	[CompilerGenerated]
	internal class NamespaceDoc
	{
	}
}
