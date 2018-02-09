using Newtonsoft.Json;

namespace Model
{
	[JsonObject]
	public class LinePushMessages
	{
		[JsonProperty("to")]
		public string To { get; set; }

		[JsonProperty("messages")]
		public Message[] Messages { get; set; }
	}

	[JsonObject]
	public class Message
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}
}