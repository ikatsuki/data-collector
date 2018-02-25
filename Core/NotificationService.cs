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

		public async Task<string> PushDailyReportAsync(IEnumerable<Report> reports)
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

		private static string[] GenerateMessage(IEnumerable<Report> reports)
		{
			var message = string.Empty;
			int method1Win = 0, method1All = 0, method2Win = 0, method2All = 0, method3Win = 0, method3All = 0;
			foreach (var r in reports)
			{
				if (r.Method == 1)
				{
					if (r.Events == null)
					{
						message += $"M1 - {r.DetailUrl}\n";
						continue;
					}

					method1All += 1;
					if (r.Method == 1 && r.Events.Any(e => e.GoalTime > r.Time && e.GoalTime <= 45))
					{
						method1Win += 1;
						message += $"M1 o {r.DetailUrl}\n";
					}
					else
					{
						message += $"M1 x {r.DetailUrl}\n";
					}
				}
				else if (r.Method == 2)
				{
					if (r.Events == null)
					{
						message += $"M2 - {r.DetailUrl}\n";
						continue;
					}

					method2All += 1;
					if (r.Method == 2 && r.Events.Any(e => e.GoalTime > r.Time && e.GoalTime <= 90))
					{
						method2Win += 1;
						message += $"M2 o {r.DetailUrl}\n";
					}
					else
					{
						message += $"M2 x {r.DetailUrl}\n";
					}
				}
				else if (r.Method == 3)
				{
					if (r.Events == null)
					{
						message += $"M3 - {r.DetailUrl}\n";
						continue;
					}

					method3All += 1;
					if (r.Method == 3 && r.Events.Any(e => e.GoalTime > r.Time && e.GoalTime <= 45))
					{
						method3Win += 1;
						message += $"M3 o {r.DetailUrl}\n";
					}
					else
					{
						message += $"M3 x {r.DetailUrl}\n";
					}
				}
			}

			if (method1All != 0) message += $"M1 勝率： {method1Win}/{method1All} = {(decimal)method1Win / method1All:P0}\n";
			if (method2All != 0) message += $"M2 勝率： {method2Win}/{method2All} = {(decimal)method2Win / method2All:P0}\n";
			if (method3All != 0) message += $"M3 勝率： {method3Win}/{method3All} = {(decimal)method3Win / method3All:P0}\n";
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