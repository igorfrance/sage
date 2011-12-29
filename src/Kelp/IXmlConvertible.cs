namespace Kelp
{
	using System.Xml;

	/// <summary>
	/// Defines the interface for objects that support simple conversion to XML.
	/// </summary>
	public interface IXmlConvertible
	{
		/// <summary>
		/// Returns an <see cref="XmlElement"/> that represents the current object.
		/// </summary>
		/// <param name="ownerDoc">The <see cref="XmlDocument"/> to use to create XML elements.</param>
		/// <returns>An <see cref="XmlElement"/> that represents the current object.</returns>
		XmlElement ToXml(XmlDocument ownerDoc);
	}
}
