using System.Collections.Generic;

namespace Model
{
	public class Report
	{
		public string Date { get; set; }
		public string Id { get; set; }
		public int Method { get; set; }
		public string Category { get; set; }
		public int Time { get; set; }
		public string HomeTeam { get; set; }
		public string AwayTeam { get; set; }
		public int HomeScore { get; set; }
		public int AwayScore { get; set; }
		public decimal HomeOdds { get; set; }
		public decimal DrawOdds { get; set; }
		public decimal AwayOdds { get; set; }
		public ICollection<GameEvent> Events { get; set; }
		public int HomeAttacks { get; set; }
		public int AwayAttacks { get; set; }
		public int HomeDangerousAttacks { get; set; }
		public int AwayDangerousAttacks { get; set; }
		public int HomeOnTarget { get; set; }
		public int AwayOnTarget { get; set; }
		public int HomeOffTarget { get; set; }
		public int AwayOffTarget { get; set; }
		public int HomePossession { get; set; }
		public int AwayPossession { get; set; }
		public decimal HomeAvgHalfGoals { get; set; }
		public decimal AwayAvgHalfGoals { get; set; }
		public decimal HomeAvgGoals { get; set; }
		public decimal AwayAvgGoals { get; set; }
		public decimal HomeAvgHalfLosts { get; set; }
		public decimal AwayAvgHalfLosts { get; set; }
		public decimal HomeAvgLosts { get; set; }
		public decimal AwayAvgLosts { get; set; }
		public string DetailUrl { get; set; }
		public bool? Win { get; set; }
	}
}