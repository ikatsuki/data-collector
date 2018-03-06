using Newtonsoft.Json;

namespace Core
{
	public class Event
	{
		[JsonProperty("time")]
		public string Time { get; set; }
	}
}