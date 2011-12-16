namespace Sage.Modules
{
	using System.Xml;

	public interface IModuleFactory
	{
		IModule CreateModule(XmlElement moduleElement);
	}
}