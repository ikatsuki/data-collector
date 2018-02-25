using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Model;

namespace SoccerDataReporter
{
	public class SoccerDataAccessor
	{
		private static string DynamodbDailyReportTableName => "SoccerDailyReport";

		public async Task<IList<Report>> GetGamesForReportAsync(string date)
		{
			var request = new QueryRequest
			{
				TableName = DynamodbDailyReportTableName,
				KeyConditionExpression = "#D = :d",
				ExpressionAttributeNames = new Dictionary<string, string>(1) {{"#D", nameof(Report.Date)}},
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>(1) {{":d", new AttributeValue {S = date}}},
				ConsistentRead = true
			};

			using (var client = new AmazonDynamoDBClient())
			{
				var response = await client.QueryAsync(request);
				return response.Items
					.Select(i =>
						new Report
						{
							Date = i[nameof(Report.Date)].S,
							Id = i[nameof(Report.Id)].S,
							Method = i.TryGetValue(nameof(Report.Method), out AttributeValue method) ? int.Parse(method.N) : default(int),
							Category = i.TryGetValue(nameof(Report.Category), out AttributeValue category) ? category.S : default(string),
							Time = int.Parse(i[nameof(Report.Time)].N),
							HomeTeam = i.TryGetValue(nameof(Report.HomeTeam), out AttributeValue homeTeam) ? homeTeam.S : default(string),
							AwayTeam = i.TryGetValue(nameof(Report.AwayTeam), out AttributeValue awayTeam) ? awayTeam.S : default(string),
							HomeScore = i.TryGetValue(nameof(Report.HomeScore), out AttributeValue homeScore) ? int.Parse(homeScore.N) : default(int),
							AwayScore = i.TryGetValue(nameof(Report.AwayScore), out AttributeValue awayScore) ? int.Parse(awayScore.N) : default(int),
							HomeOdds = i.TryGetValue(nameof(Report.HomeOdds), out AttributeValue homeOdds) ? decimal.Parse(homeOdds.S) : default(decimal),
							DrawOdds = i.TryGetValue(nameof(Report.DrawOdds), out AttributeValue drawOdds) ? decimal.Parse(drawOdds.S) : default(decimal),
							AwayOdds = i.TryGetValue(nameof(Report.AwayOdds), out AttributeValue awayOdds) ? decimal.Parse(awayOdds.S) : default(decimal),
							HomeAttacks = i.TryGetValue(nameof(Report.HomeAttacks), out AttributeValue homeAttacks) ? int.Parse(homeAttacks.N) : default(int),
							AwayAttacks = i.TryGetValue(nameof(Report.AwayAttacks), out AttributeValue awayAttacks) ? int.Parse(awayAttacks.N) : default(int),
							HomeDangerousAttacks = i.TryGetValue(nameof(Report.HomeDangerousAttacks), out AttributeValue homeDangerousAttacks) ? int.Parse(homeDangerousAttacks.N) : default(int),
							AwayDangerousAttacks = i.TryGetValue(nameof(Report.AwayDangerousAttacks), out AttributeValue awayDangerousAttacks) ? int.Parse(awayDangerousAttacks.N) : default(int),
							HomeOnTarget = i.TryGetValue(nameof(Report.HomeOnTarget), out AttributeValue homeOnTarget) ? int.Parse(homeOnTarget.N) : default(int),
							AwayOnTarget = i.TryGetValue(nameof(Report.AwayOnTarget), out AttributeValue awayOnTarget) ? int.Parse(awayOnTarget.N) : default(int),
							HomeOffTarget = i.TryGetValue(nameof(Report.HomeOffTarget), out AttributeValue homeOffTarget) ? int.Parse(homeOffTarget.N) : default(int),
							AwayOffTarget = i.TryGetValue(nameof(Report.AwayOffTarget), out AttributeValue awayOffTarget) ? int.Parse(awayOffTarget.N) : default(int),
							HomePossession = i.TryGetValue(nameof(Report.HomePossession), out AttributeValue homePossession) ? int.Parse(homePossession.N) : default(int),
							AwayPossession = i.TryGetValue(nameof(Report.AwayPossession), out AttributeValue awayPossession) ? int.Parse(awayPossession.N) : default(int),
							HomeAvgHalfGoals = i.TryGetValue(nameof(Report.HomeAvgHalfGoals), out AttributeValue homeAvgHalfGoals) ? decimal.Parse(homeAvgHalfGoals.S) : default(decimal),
							AwayAvgHalfGoals = i.TryGetValue(nameof(Report.AwayAvgHalfGoals), out AttributeValue awayAvgHalfGoals) ? decimal.Parse(awayAvgHalfGoals.S) : default(decimal),
							HomeAvgGoals = i.TryGetValue(nameof(Report.HomeAvgGoals), out AttributeValue homeAvgGoals) ? decimal.Parse(homeAvgGoals.S) : default(decimal),
							AwayAvgGoals = i.TryGetValue(nameof(Report.AwayAvgGoals), out AttributeValue awayAvgGoals) ? decimal.Parse(awayAvgGoals.S) : default(decimal),
							HomeAvgHalfLosts = i.TryGetValue(nameof(Report.HomeAvgHalfLosts), out AttributeValue homeAvgHalfLosts) ? decimal.Parse(homeAvgHalfLosts.S) : default(decimal),
							AwayAvgHalfLosts = i.TryGetValue(nameof(Report.AwayAvgHalfLosts), out AttributeValue awayAvgHalfLosts) ? decimal.Parse(awayAvgHalfLosts.S) : default(decimal),
							HomeAvgLosts = i.TryGetValue(nameof(Report.HomeAvgLosts), out AttributeValue homeAvgLosts) ? decimal.Parse(homeAvgLosts.S) : default(decimal),
							AwayAvgLosts = i.TryGetValue(nameof(Report.AwayAvgLosts), out AttributeValue awayAvgLosts) ? decimal.Parse(awayAvgLosts.S) : default(decimal),
							DetailUrl = i[nameof(Report.DetailUrl)].S,
						}).ToList();
			}
		}
	}
}