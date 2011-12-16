namespace Sage.Controllers
{
	using System;

	/// <summary>
	/// Indicates that the controller this attribute is decorating, is a shared controllers; that is, it's resources are retrieved
	/// from the shared directory, rather than attempting to resolve the current category and use the resources from there.
	/// </summary>
	public class SharedControllerAttribute : Attribute
	{
	}
}
