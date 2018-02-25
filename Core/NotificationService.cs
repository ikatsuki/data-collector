using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Model;

namespace Core
{
	public class NotificationService
	{
		private static string LineApiRootEndpoint => "https://api.line.me";
		private static string LineApiPushMessageEndpoint => LineApiRootEndpoint + "/v2/bot/message/push";

		public async Task PushMessagesAsync(IList<Game> games)
		{
			const string type = "text";
			var messages = GenerateMessages(games);

			while (messages.Any())
			{
				var lineMessage = new LinePushMessages
				{
					To = Environment.GetEnvironmentVariable("UserId"),
					Messages = messages.Take(5).Select(m => new Message {Type = type, Text = m}).ToArray()
				};

				var json = JsonConvert.SerializeObject(lineMessage);

				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.DefaultRequestHeaders.Authorization =
						AuthenticationHeaderValue.Parse($"Bearer {Environment.GetEnvironmentVariable("LineAccessToken")}");
					var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
					var response = await client.PostAsync(LineApiPushMessageEndpoint, jsonContent);
					if (response.StatusCode != HttpStatusCode.OK)
						throw new HttpRequestException($"{response.StatusCode} {response.Content.ReadAsStringAsync()}");
				}

				messages = messages.Skip(5).ToList();
			}
		}

		public IList<string> GenerateMessages(IList<Game> games)
		{
			return games.Select(game => $@"{game.DetailUrl}
{game.Category}
{game.HomeTeam} vs {game.AwayTeam}
Method {game.Method}
{game.Time}' {game.HomeScore}-{game.AwayScore}
Od: {game.HomeOdds}-{game.DrawOdds}-{game.AwayOdds}
Cn: {game.HomeCorners}-{game.AwayCorners}
Red: {game.HomeRedCard}-{game.AwayRedCard}
A: {game.HomeAttacks}-{game.AwayAttacks}
DA: {game.HomeDangerousAttacks}-{game.AwayDangerousAttacks}
OnT: {game.HomeOnTarget}-{game.AwayOnTarget}
OfT: {game.HomeOffTarget}-{game.AwayOffTarget}
Pos: {game.HomePossession}-{game.AwayPossession}
AvgHG:{game.HomeAvgHalfGoals}-{game.AwayAvgHalfGoals}
AvgG: {game.HomeAvgGoals}-{game.AwayAvgGoals}
AvgHL:{game.HomeAvgHalfLosts}-{game.AwayAvgHalfLosts}
AvgL: {game.HomeAvgLosts}-{game.AwayAvgLosts}").ToList();
		}

		public async Task<string> PushDailyReportAsync(IList<Report> reports)
		{
			const string type = "text";
			var messages = GenerateMessage(reports);
			if (messages.All(string.IsNullOrEmpty))
				return null;

			var lineMessage = new LinePushMessages
			{
				To = Environment.GetEnvironmentVariable("UserId"),
				Messages = messages.Select(m => new Message { Type = type, Text = m }).ToArray()
			};

			var json = JsonConvert.SerializeObject(lineMessage);

			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Authorization =
					AuthenticationHeaderValue.Parse($"Bearer {Environment.GetEnvironmentVariable("LineAccessToken")}");
				var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(LineApiPushMessageEndpoint, jsonContent);
				if (response.StatusCode != HttpStatusCode.OK)
					throw new HttpRequestException($"{response.StatusCode} {response.Content.ReadAsStringAsync()}");
			}

			return json;
		}

		private static string[] GenerateMessage(IList<Report> reports)
		{
			var message = string.Empty;
			var method1Win = reports.Count(r => r.Method == 1 && (r.Win ?? false));
			var method1All = reports.Count(r => r.Method == 1);
			var method2Win = reports.Count(r => r.Method == 2 && (r.Win ?? false));
			var method2All = reports.Count(r => r.Method == 2);
			var method3Win = reports.Count(r => r.Method == 3 && (r.Win ?? false));
			var method3All = reports.Count(r => r.Method == 3);

			if (method1All != 0) message += $"M1 勝率： {method1Win}/{method1All} = {(decimal)method1Win / method1All:P0}\n";
			if (method2All != 0) message += $"M2 勝率： {method2Win}/{method2All} = {(decimal)method2Win / method2All:P0}\n";
			if (method3All != 0) message += $"M3 勝率： {method3Win}/{method3All} = {(decimal)method3Win / method3All:P0}\n";

			foreach (var r in reports)
			{
				if(r.Win == null)
					continue;

				message += $"M{r.Method} {((bool)r.Win ? "o" : "x")} {r.Category} {r.HomeTeam} {r.HomeScore} vs {r.AwayScore} {r.AwayTeam}\n";
			}

			return SubstringAtCount(message, 2000);
		}

		public static string[] SubstringAtCount(string self, int count)
		{
			var result = new List<string>();
			var length = (int)Math.Ceiling((double)self.Length / count);

			for (var i = 0; i < length; i++)
			{
				var start = count * i;
				if (self.Length <= start)
				{
					break;
				}
				result.Add(self.Length < start + count ? self.Substring(start) : self.Substring(start, count));
			}

			return result.ToArray();
		}
	}
}