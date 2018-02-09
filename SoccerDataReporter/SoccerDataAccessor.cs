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

		public async Task<IEnumerable<Report>> GetGamesForReportAsync(string date)
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
							DetailUrl = i[nameof(Report.DetailUrl)].S,
							Time = int.Parse(i[nameof(Report.Time)].N),
							HomeScore = i.TryGetValue(nameof(Report.HomeScore), out AttributeValue homeScore) ? int.Parse(homeScore.N) : default(int),
							AwayScore = i.TryGetValue(nameof(Report.AwayScore), out AttributeValue awayScore) ? int.Parse(awayScore.N) : default(int),
							HomeOdds = i.TryGetValue(nameof(Report.HomeOdds), out AttributeValue homeOdds) ? decimal.Parse(homeOdds.S) : default(decimal),
							DrawOdds = i.TryGetValue(nameof(Report.DrawOdds), out AttributeValue drawOdds) ? decimal.Parse(drawOdds.S) : default(decimal),
							AwayOdds = i.TryGetValue(nameof(Report.HomeOdds), out AttributeValue awayOdds) ? decimal.Parse(awayOdds.S) : default(decimal),
							Category = i.TryGetValue(nameof(Report.Category), out AttributeValue category) ? category.S : default(string),
						});
			}
		}
	}
}