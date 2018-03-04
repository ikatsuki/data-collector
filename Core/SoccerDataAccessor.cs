using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Model;

namespace Core
{
	public class SoccerDataAccessor
	{
		private static string DynamodbStatsTableName => "SoccerStats";
		private static string DynamodbReportTableName => "SoccerDailyReport";

		public async Task<PutItemResponse[]> PutGamesAsync(IEnumerable<Game> games)
		{
			var now = DateTimeOffset.UtcNow;
			var targetGamesTasks = games.Select(async game =>
			{
				using (var client = new AmazonDynamoDBClient())
				{
					var request = new PutItemRequest
					{
						TableName = DynamodbStatsTableName,
						Item = new Dictionary<string, AttributeValue>
						{
							{nameof(Game.Id), new AttributeValue {S = game.Id}},
							{nameof(Game.Method), new AttributeValue {S = game.Method.ToString()}},
							{"CreatedAt", new AttributeValue {S = now.ToString("u")}},
							{nameof(Game.Category), new AttributeValue {S = game.Category}},
							{nameof(Game.Time), new AttributeValue {N = game.Time.ToString()}},
							{nameof(Game.HomeTeam), new AttributeValue {S = game.HomeTeam}},
							{nameof(Game.AwayTeam), new AttributeValue {S = game.AwayTeam}},
							{nameof(Game.HomeScore), new AttributeValue {N = game.HomeScore.ToString()}},
							{nameof(Game.AwayScore), new AttributeValue {N = game.AwayScore.ToString()}},
							{nameof(Game.HomeOdds), new AttributeValue {N = game.HomeOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.DrawOdds), new AttributeValue {N = game.DrawOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayOdds), new AttributeValue {N = game.AwayOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.HomeCorners), new AttributeValue {N = game.HomeCorners.ToString()}},
							{nameof(Game.AwayCorners), new AttributeValue {N = game.AwayCorners.ToString()}},
							{nameof(Game.HomeRedCard), new AttributeValue {N = game.HomeRedCard.ToString()}},
							{nameof(Game.AwayRedCard), new AttributeValue {N = game.AwayRedCard.ToString()}},
							{nameof(Game.HomeAttacks), new AttributeValue {N = game.HomeAttacks.ToString()}},
							{nameof(Game.AwayAttacks), new AttributeValue {N = game.AwayAttacks.ToString()}},
							{nameof(Game.HomeDangerousAttacks), new AttributeValue {N = game.HomeDangerousAttacks.ToString()}},
							{nameof(Game.AwayDangerousAttacks), new AttributeValue {N = game.AwayDangerousAttacks.ToString()}},
							{nameof(Game.HomeOnTarget), new AttributeValue {N = game.HomeOnTarget.ToString()}},
							{nameof(Game.AwayOnTarget), new AttributeValue {N = game.AwayOnTarget.ToString()}},
							{nameof(Game.HomeOffTarget), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayOffTarget), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomePossession), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayPossession), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomeAvgHalfGoals), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayAvgHalfGoals), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomeAvgGoals), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayAvgGoals), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomeAvgHalfLosts), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayAvgHalfLosts), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomeAvgLosts), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayAvgLosts), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.DetailUrl), new AttributeValue {S = game.DetailUrl}},
						}
					};
					return await client.PutItemAsync(request);
				}
			});
			return await Task.WhenAll(targetGamesTasks);
		}

		public async Task<PutItemResponse[]> PutReportAsync(IEnumerable<Game> games)
		{
			var date = $"{DateTime.Now:yyyyMMdd}";
			var targetGamesTasks = games.Select(async game =>
			{
				using (var client = new AmazonDynamoDBClient())
				{
					var request = new PutItemRequest
					{
						TableName = DynamodbReportTableName,
						Item = new Dictionary<string, AttributeValue>
						{
							{nameof(Report.Date), new AttributeValue {S = date}},
							{nameof(Report.Id), new AttributeValue {S = game.Id}},
							{nameof(Report.Method), new AttributeValue {N = game.Method.ToString()}},
							{nameof(Game.Category), new AttributeValue {S = game.Category}},
							{nameof(Game.Time), new AttributeValue {N = game.Time.ToString()}},
							{nameof(Game.HomeTeam), new AttributeValue {S = game.HomeTeam}},
							{nameof(Game.AwayTeam), new AttributeValue {S = game.AwayTeam}},
							{nameof(Game.HomeScore), new AttributeValue {N = game.HomeScore.ToString()}},
							{nameof(Game.AwayScore), new AttributeValue {N = game.AwayScore.ToString()}},
							{nameof(Game.HomeOdds), new AttributeValue {N = game.HomeOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.DrawOdds), new AttributeValue {N = game.DrawOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayOdds), new AttributeValue {N = game.AwayOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.HomeCorners), new AttributeValue {N = game.HomeCorners.ToString()}},
							{nameof(Game.AwayCorners), new AttributeValue {N = game.AwayCorners.ToString()}},
							{nameof(Game.HomeRedCard), new AttributeValue {N = game.HomeRedCard.ToString()}},
							{nameof(Game.AwayRedCard), new AttributeValue {N = game.AwayRedCard.ToString()}},
							{nameof(Game.HomeAttacks), new AttributeValue {N = game.HomeAttacks.ToString()}},
							{nameof(Game.AwayAttacks), new AttributeValue {N = game.AwayAttacks.ToString()}},
							{nameof(Game.HomeDangerousAttacks), new AttributeValue {N = game.HomeDangerousAttacks.ToString()}},
							{nameof(Game.AwayDangerousAttacks), new AttributeValue {N = game.AwayDangerousAttacks.ToString()}},
							{nameof(Game.HomeOnTarget), new AttributeValue {N = game.HomeOnTarget.ToString()}},
							{nameof(Game.AwayOnTarget), new AttributeValue {N = game.AwayOnTarget.ToString()}},
							{nameof(Game.HomeOffTarget), new AttributeValue {N = game.HomeOffTarget.ToString()}},
							{nameof(Game.AwayOffTarget), new AttributeValue {N = game.AwayOffTarget.ToString()}},
							{nameof(Game.HomePossession), new AttributeValue {N = game.HomePossession.ToString()}},
							{nameof(Game.AwayPossession), new AttributeValue {N = game.AwayPossession.ToString()}},
							{nameof(Game.HomeAvgHalfGoals), new AttributeValue {N = game.HomeAvgHalfGoals.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayAvgHalfGoals), new AttributeValue {N = game.AwayAvgHalfGoals.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.HomeAvgGoals), new AttributeValue {N = game.HomeAvgGoals.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayAvgGoals), new AttributeValue {N = game.AwayAvgGoals.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.HomeAvgHalfLosts), new AttributeValue {N = game.HomeAvgHalfLosts.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayAvgHalfLosts), new AttributeValue {N = game.AwayAvgHalfLosts.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.HomeAvgLosts), new AttributeValue {N = game.HomeAvgLosts.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.AwayAvgLosts), new AttributeValue {N = game.AwayAvgLosts.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Game.DetailUrl), new AttributeValue {S = game.DetailUrl}},
						}
					};
					return await client.PutItemAsync(request);
				}
			});
			return await Task.WhenAll(targetGamesTasks);
		}

		public async Task<string> GetGameId(string id)
		{
			var request = new QueryRequest
			{
				TableName = DynamodbStatsTableName,
				KeyConditionExpression = "#id = :id",
				ExpressionAttributeNames = new Dictionary<string, string>(1) {{"#id", "Id"}},
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>(1) {{":id", new AttributeValue {S = id}}},
				ScanIndexForward = false,
				ProjectionExpression = "Id",
				Limit = 1
			};
			
			var dynamoDbClient = new AmazonDynamoDBClient();
			var response = await dynamoDbClient.QueryAsync(request);
			return response.Items.FirstOrDefault()?[nameof(Game.Id)]?.S;
		}

		public async Task<IList<Report>> GetGamesForReportAsync(string date)
		{
			var request = new QueryRequest
			{
				TableName = DynamodbReportTableName,
				KeyConditionExpression = "#D = :d",
				ExpressionAttributeNames = new Dictionary<string, string>(1) { { "#D", nameof(Report.Date) } },
				ExpressionAttributeValues = new Dictionary<string, AttributeValue>(1) { { ":d", new AttributeValue { S = date } } },
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
							HomeOdds = i.TryGetValue(nameof(Report.HomeOdds), out AttributeValue homeOdds) ? decimal.Parse(homeOdds.N ?? homeOdds.S) : default(decimal),
							DrawOdds = i.TryGetValue(nameof(Report.DrawOdds), out AttributeValue drawOdds) ? decimal.Parse(drawOdds.N ?? drawOdds.S) : default(decimal),
							AwayOdds = i.TryGetValue(nameof(Report.AwayOdds), out AttributeValue awayOdds) ? decimal.Parse(awayOdds.N ?? awayOdds.S) : default(decimal),
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
							HomeAvgHalfGoals = i.TryGetValue(nameof(Report.HomeAvgHalfGoals), out AttributeValue homeAvgHalfGoals) ? decimal.Parse(homeAvgHalfGoals.N) : default(decimal),
							AwayAvgHalfGoals = i.TryGetValue(nameof(Report.AwayAvgHalfGoals), out AttributeValue awayAvgHalfGoals) ? decimal.Parse(awayAvgHalfGoals.N) : default(decimal),
							HomeAvgGoals = i.TryGetValue(nameof(Report.HomeAvgGoals), out AttributeValue homeAvgGoals) ? decimal.Parse(homeAvgGoals.N) : default(decimal),
							AwayAvgGoals = i.TryGetValue(nameof(Report.AwayAvgGoals), out AttributeValue awayAvgGoals) ? decimal.Parse(awayAvgGoals.N) : default(decimal),
							HomeAvgHalfLosts = i.TryGetValue(nameof(Report.HomeAvgHalfLosts), out AttributeValue homeAvgHalfLosts) ? decimal.Parse(homeAvgHalfLosts.N) : default(decimal),
							AwayAvgHalfLosts = i.TryGetValue(nameof(Report.AwayAvgHalfLosts), out AttributeValue awayAvgHalfLosts) ? decimal.Parse(awayAvgHalfLosts.N) : default(decimal),
							HomeAvgLosts = i.TryGetValue(nameof(Report.HomeAvgLosts), out AttributeValue homeAvgLosts) ? decimal.Parse(homeAvgLosts.N) : default(decimal),
							AwayAvgLosts = i.TryGetValue(nameof(Report.AwayAvgLosts), out AttributeValue awayAvgLosts) ? decimal.Parse(awayAvgLosts.N) : default(decimal),
							DetailUrl = i[nameof(Report.DetailUrl)].S,
						}).ToList();
			}
		}
	}
}