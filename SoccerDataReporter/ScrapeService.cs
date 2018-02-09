using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Model;

namespace SoccerDataReporter
{
	public class ScrapeService
	{
		public async Task<IEnumerable<Report>> GetGameEventsAsync(IEnumerable<Report> games)
		{
			games = games.ToList();
			foreach (var game in games)
			{
				var gameDetail = await GetHtmlDocumentAsync(game.DetailUrl);
				game.Events = gameDetail.QuerySelectorAll(".panel > .panel-heading > .panel-title")
					.Where(e => e.TextContent.Contains("Events"))
					.Select(e => e.ParentElement.ParentElement)
					.Where(e => e.ChildElementCount > 1)
					.Select(e => e.Children.Last())
					.FirstOrDefault()?
					.Children
					.Where(c => c.TextContent.Contains(" Goal "))
					.Select(c => Regex.Replace(c.TextContent.Replace("-", ""), "\\+\\d", "").Split('\''))
					.Select(e => new GameEvent
					{
						GoalTime = int.TryParse(e[0]?.Trim(), out int time) ? time : 0,
						Team = e[1]?.Trim()
					}).ToList();
			}

			return games;
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
	}
}