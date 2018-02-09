using System.Collections.Generic;

namespace Model
{
	public class Report
	{
		public string Date { get; set; }
		public string Id { get; set; }
		public int Method { get; set; }
		public string DetailUrl { get; set; }
		public int Time { get; set; }
		public int HomeScore { get; set; }
		public int AwayScore { get; set; }
		public decimal HomeOdds { get; set; }
		public decimal DrawOdds { get; set; }
		public decimal AwayOdds { get; set; }
		public string Category { get; set; }
		public ICollection<GameEvent> Events { get; set; }
	}
}