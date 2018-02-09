using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using AngleSharp.Dom.Html;
using Core.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace SoccerDataCollector
{
	public class Function
	{
		/// <summary>
		///     A simple function that takes a string and does a ToUpper
		/// </summary>
		/// <param name="input"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task<dynamic> FunctionHandler(dynamic input, ILambdaContext context)
		{
			var a = new NotifySoccerBetTiming.Function();
			var b = new SoccerBetReport.Function();

			return input?.ToUpper();
		}

		private static async Task<IEnumerable<Game>> GetInPlayGamesTask(IHtmlDocument doc)
		{
			var inPlayElement = doc.GetElementById("tbl_inplay");
			var getGamesTasks = inPlayElement.GetElementsByClassName("c_1")
				.Select(async element =>
				{
					var id = element.Id.Split('_')[1];
					var gameElements = element.GetElementsByTagName("td");
					var scores = gameElements[3].TextContent.Split('-');
					var game = new Game
					{
						Id = id,
						Category = gameElements[0].TextContent,
						Time = int.Parse(gameElements[1].QuerySelector(".race-time").TextContent.Trim().Substring(0, 2)),
						HomeTeam = gameElements[2].QuerySelector("a").TextContent,
						AwayTeam = gameElements[4].QuerySelector("a").TextContent,
						HomeScore = int.Parse(scores[0]),
						AwayScore = int.Parse(scores[1]),
						DetailUrl = BetsApiRootEndpoint + gameElements[3].QuerySelector("a").GetAttribute("href")
					};

					if (decimal.TryParse(gameElements[5].TextContent, out decimal homeOdds))
					{
						game.HomeOdds = homeOdds;
						game.DrawOdds = decimal.Parse(gameElements[6].TextContent);
						game.AwayOdds = decimal.Parse(gameElements[7].TextContent);
					}
					;

					var gameDetail = await GetHtmlDocumentAsync(game.DetailUrl);
					var details = gameDetail.GetElementsByClassName("table");
					var infos = details[0].GetElementsByTagName("tr");
					foreach (var info in infos)
						SetDetail(info.GetElementsByTagName("td"), ref game);

					SetGameEvents(gameDetail, ref game);

					var teamAnalytics5Element = gameDetail.QuerySelector("#analyticsR5 tbody")?.GetElementsByTagName("tr");
					if (teamAnalytics5Element != null)
					{
						var homeAnalytics = teamAnalytics5Element[0].GetElementsByTagName("td");
						if (decimal.TryParse(homeAnalytics[0].TextContent, out decimal homeAvgHalfGoals))
							game.HomeAvgHalfGoals = homeAvgHalfGoals;

						if (decimal.TryParse(homeAnalytics[1].TextContent, out decimal homeAvgGoals))
							game.HomeAvgGoals = decimal.Parse(homeAnalytics[1].TextContent);

						if (decimal.TryParse(homeAnalytics[2].TextContent, out decimal homeAvgHalfLosts))
							game.HomeAvgHalfLosts = decimal.Parse(homeAnalytics[2].TextContent);

						if (decimal.TryParse(homeAnalytics[3].TextContent, out decimal homeAvgLosts))
							game.HomeAvgLosts = decimal.Parse(homeAnalytics[3].TextContent);

						var awayAnalytics = teamAnalytics5Element[1].GetElementsByTagName("td");
						if (decimal.TryParse(awayAnalytics[0].TextContent, out decimal awayAvgHalfGoals))
							game.AwayAvgHalfGoals = decimal.Parse(awayAnalytics[0].TextContent);

						if (decimal.TryParse(awayAnalytics[1].TextContent, out decimal awayAvgGoals))
							game.AwayAvgGoals = decimal.Parse(awayAnalytics[1].TextContent);

						if (decimal.TryParse(awayAnalytics[2].TextContent, out decimal awayAvgHalfLosts))
							game.AwayAvgHalfLosts = decimal.Parse(awayAnalytics[2].TextContent);

						if (decimal.TryParse(awayAnalytics[3].TextContent, out decimal awayAvgLosts))
							game.AwayAvgLosts = decimal.Parse(awayAnalytics[3].TextContent);
					}

					return game;
				});
			return await Task.WhenAll(getGamesTasks);
		}
	}
}