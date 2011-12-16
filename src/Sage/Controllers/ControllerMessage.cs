namespace Sage.Controllers
{
	/// <summary>
	/// Represents a single message that a controller is sending to the view.
	/// </summary>
	public class ControllerMessage
	{
		/// <summary>
		/// Gets or sets the type of this message.
		/// </summary>
		public MessageType Type { get; set; }

		/// <summary>
		/// Gets or sets the name of this message.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the text of this message.
		/// </summary>
		public string Text { get; set; }
	}
}
