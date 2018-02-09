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

namespace SoccerDataCollector
{
	class NotificationService
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
	}
}