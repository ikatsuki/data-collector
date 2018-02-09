using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Model;

namespace SoccerDataCollector
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
			var targetGamesTasks = games.Select(async game =>
			{
				using (var client = new AmazonDynamoDBClient())
				{
					var request = new PutItemRequest
					{
						TableName = DynamodbReportTableName,
						Item = new Dictionary<string, AttributeValue>
						{
							{nameof(Report.Date), new AttributeValue {S = game.Id}},
							{nameof(Report.Method), new AttributeValue {N = game.Method.ToString()}},
							{nameof(Report.Time), new AttributeValue {N = game.Time.ToString()}},
							{nameof(Report.Id), new AttributeValue {S = game.Id.ToString()}},
							{nameof(Report.DetailUrl), new AttributeValue {S = game.DetailUrl}},
							{nameof(Report.HomeScore), new AttributeValue {N = game.HomeScore.ToString()}},
							{nameof(Report.AwayScore), new AttributeValue {N = game.AwayScore.ToString()}},
							{nameof(Report.HomeOdds), new AttributeValue {S = game.HomeOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Report.DrawOdds), new AttributeValue {S = game.DrawOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Report.AwayOdds), new AttributeValue {S = game.AwayOdds.ToString(CultureInfo.InvariantCulture)}},
							{nameof(Report.Category), new AttributeValue {S = game.Category}},
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
	}
}