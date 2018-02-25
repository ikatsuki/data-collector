using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Model;

namespace SoccerDataCollector
{
	public class ScrapeService
	{
		private static string BetsApiRootEndpoint => "https://betsapi.com";
		private static string BetsApiSoccerInPlayEndpoint => BetsApiRootEndpoint + "/ci/soccer";

		public async Task<IList<Game>> GetInPlayGamesTask()
		{
			var doc = await GetHtmlDocumentAsync(BetsApiSoccerInPlayEndpoint);
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

					var gameDetail = await GetHtmlDocumentAsync(game.DetailUrl);
					var details = gameDetail.GetElementsByClassName("table");
					var infos = details[0].GetElementsByTagName("tr");
					foreach (var info in infos)
						SetDetail(info.GetElementsByTagName("td"), ref game);

					SetGameEvents(gameDetail, ref game);

					var teamAnalytics5Element = gameDetail.QuerySelector("#analyticsR5 tbody")?.GetElementsByTagName("tr");
					if (teamAnalytics5Element == null) return game;

					var homeAnalytics = teamAnalytics5Element[0].GetElementsByTagName("td");
					game.HomeAvgHalfGoals = decimal.TryParse(homeAnalytics[0].TextContent, out decimal homeAvgHalfGoals)
						? homeAvgHalfGoals
						: 0;
					game.HomeAvgGoals = decimal.TryParse(homeAnalytics[1].TextContent, out decimal homeAvgGoals) ? homeAvgGoals : 0;
					game.HomeAvgHalfLosts = decimal.TryParse(homeAnalytics[2].TextContent, out decimal homeAvgHalfLosts)
						? homeAvgHalfLosts
						: 0;
					game.HomeAvgLosts = decimal.TryParse(homeAnalytics[3].TextContent, out decimal homeAvgLosts) ? homeAvgLosts : 0;

					var awayAnalytics = teamAnalytics5Element[1].GetElementsByTagName("td");
					game.AwayAvgHalfGoals = decimal.TryParse(awayAnalytics[0].TextContent, out decimal awayAvgHalfGoals)
						? awayAvgHalfGoals
						: 0;
					game.AwayAvgGoals = decimal.TryParse(awayAnalytics[1].TextContent, out decimal awayAvgGoals) ? awayAvgGoals : 0;
					game.AwayAvgHalfLosts = decimal.TryParse(awayAnalytics[2].TextContent, out decimal awayAvgHalfLosts)
						? awayAvgHalfLosts
						: 0;
					game.AwayAvgLosts = decimal.TryParse(awayAnalytics[3].TextContent, out decimal awayAvgLosts) ? awayAvgLosts : 0;

					return game;
				});
			return await Task.WhenAll(getGamesTasks);
		}

		private static async Task<IHtmlDocument> GetHtmlDocumentAsync(string url)
		{
			using (var client = new HttpClient())
			using (var stream = await client.GetStreamAsync(url))
			{
				var parser = new HtmlParser();
				return await parser.ParseAsync(stream);
			}
		}

		private static void SetDetail(IHtmlCollection<IElement> info, ref Game game)
		{
			if(info.Length < 3) return;

			var statsName = info[1].TextContent;
			if (statsName == "Corners")
			{
				if (int.TryParse(info[0].TextContent, out int homeCorners))
					game.HomeCorners = homeCorners;

				if (int.TryParse(info[2].TextContent, out int awayCorners))
					game.AwayCorners = awayCorners;
			}
			else if (statsName == "Red Card")
			{
				if (int.TryParse(info[0].TextContent, out int homeRedCard))
					game.HomeRedCard = homeRedCard;

				if (int.TryParse(info[2].TextContent, out int awayRedCard))
					game.AwayRedCard = awayRedCard;
			}
			else if (statsName == "Attacks")
			{
				if (int.TryParse(info[0].QuerySelector(".sr-only").TextContent, out int homeAttacks))
					game.HomeAttacks = homeAttacks;

				if (int.TryParse(info[2].QuerySelector(".sr-only").TextContent, out int awayAttacks))
					game.AwayAttacks = awayAttacks;
			}
			else if (statsName == "Dangerous Attacks")
			{
				if (int.TryParse(info[0].QuerySelector(".sr-only").TextContent, out int homeDangerousAttacks))
					game.HomeDangerousAttacks = homeDangerousAttacks;

				if (int.TryParse(info[2].QuerySelector(".sr-only").TextContent, out int awayDangerousAttacks))
					game.AwayDangerousAttacks = awayDangerousAttacks;
			}
			else if (statsName == "On Target")
			{
				if (int.TryParse(info[0].QuerySelector(".sr-only").TextContent, out int homeOnTarget))
					game.HomeOnTarget = homeOnTarget;

				if (int.TryParse(info[2].QuerySelector(".sr-only").TextContent, out int awayOnTarget))
					game.AwayOnTarget = awayOnTarget;
			}
			else if (statsName == "Off Target")
			{
				if (int.TryParse(info[0].QuerySelector(".sr-only").TextContent, out int homeOffTarget))
					game.HomeOffTarget = homeOffTarget;

				if (int.TryParse(info[2].QuerySelector(".sr-only").TextContent, out int awayOffTarget))
					game.AwayOffTarget = awayOffTarget;
			}
			else if (statsName.Contains("Possession"))
			{
				if (int.TryParse(info[0].QuerySelector(".sr-only").TextContent, out int homePossession))
					game.HomePossession = homePossession;

				if (int.TryParse(info[2].QuerySelector(".sr-only").TextContent, out int awayPossession))
					game.AwayPossession = awayPossession;
			}
		}

		private static void SetGameEvents(IHtmlDocument doc, ref Game game)
		{
			game.Events = doc.QuerySelectorAll(".panel > .panel-heading > .panel-title")
				.Where(e => e.TextContent.Contains("Events"))
				.Select(e => e.ParentElement.ParentElement)
				.Where(e => e.ChildElementCount > 1)
				.Select(e => e.Children.Last())
				.FirstOrDefault()?
				.Children
				.Where(c => c.TextContent.Contains("\'") && c.TextContent.Contains(" Goal "))
				.Select(c => Regex.Replace(c.TextContent.Replace("-", ""), "\\+\\d", "").Split('\''))
				.Select(e => new GameEvent
				{
					GoalTime = int.TryParse(e[0]?.Trim(), out int time) ? time : 0,
					Team = e.Length > 1 ? e[1]?.Trim() : string.Empty
				}).ToList();
		}
	}
}