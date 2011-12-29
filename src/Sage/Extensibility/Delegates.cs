namespace Sage.Extensibility
{
	using System.Xml;

	using Sage.ResourceManagement;

	/// <summary>
	/// Defines the signature of the delegate that can return the <see cref="CacheableXmlDocument"/> for the specified
	/// <paramref name="resourceUri"/> and <paramref name="context"/>.
	/// </summary>
	/// <param name="context">The context under which the code is executing.</param>
	/// <param name="resourceUri">The URI of the resource to open.</param>
	/// <returns>A <see cref="CacheableXmlDocument"/> associated with the <paramref name="resourceUri"/>.</returns>
	public delegate CacheableXmlDocument SageResourceProvider(SageContext context, string resourceUri);

	/// <summary>
	/// Defines the signature of a method that can be used to handle a single XML node during copying of an XML document.
	/// </summary>
	/// <param name="node">The node being processed.</param>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <returns>The XML node that should be copied in the result document, or a <c>null</c> if the node should be skipped.</returns>
	/// <seealso cref="ResourceManager.CopyNode"/>
	/// <seealso cref="ResourceManager.RegisterNodeHandler"/>
	public delegate XmlNode CopyNodeHandler(XmlNode node, SageContext context);

	/// <summary>
	/// Defines the signature of a method that can be used to substitute placeholders in element or attribute text during 
	/// copying of an XML document.
	/// </summary>
	/// <param name="variableName">The name of the variable that was matched.</param>
	/// <param name="context">The context under which the method is being executed.</param>
	/// <returns>The text that should be used instead of the original text, or a <c>null</c> if the node should be skipped.</returns>
	/// <seealso cref="ResourceManager.CopyNode"/>
	/// <seealso cref="ResourceManager.RegisterTextHandler"/>
	public delegate string CopyTextHandler(string variableName, SageContext context);
}
