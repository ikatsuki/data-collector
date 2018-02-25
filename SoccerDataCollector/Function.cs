using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Core;
using Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SoccerDataCollector
{
	public class Function
	{
		private static ScrapeService ScrapeService => new ScrapeService();

		private static SoccerService SoccerService => new SoccerService();

		private static NotificationService NotificationService => new NotificationService();

		private static SoccerDataAccessor SoccerDataAccessor => new SoccerDataAccessor();

		/// <summary>
		/// A simple function that takes a string and does a ToUpper
		/// </summary>
		/// <param name="input"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task<dynamic> FunctionHandler(dynamic input, ILambdaContext context)
		{
			context.Logger.LogLine($"SoccerDataCollector start at {DateTime.Now:yyyy-MM-dd HH:mm}");
			context.Logger.LogLine($"input: {input}");

			IList<Game> inPlayGammes;

			try
			{
				inPlayGammes = await ScrapeService.GetInPlayGamesTask();
				if (inPlayGammes == null || !inPlayGammes.Any())
				{
					context.Logger.LogLine("no games");
					return "no games";
				}
			}
			catch (Exception e)
			{
				context.Logger.LogLine("happen error.");
				context.Logger.LogLine($"exception {e}");
				return "can't get games from site";
			}

			var method1Games = await SoccerService.GetMethod1GamesAsync(inPlayGammes);
			context.Logger.LogLine(string.Format("method1 game is {0}", method1Games.Count));
			await SoccerDataAccessor.PutGamesAsync(method1Games);
			await SoccerDataAccessor.PutReportAsync(method1Games);
			await NotificationService.PushMessagesAsync(method1Games);

			var method2Games = await SoccerService.GetMethod2GamesAsync(inPlayGammes);
			context.Logger.LogLine(string.Format("method2 game is {0}", method2Games.Count));
			await SoccerDataAccessor.PutGamesAsync(method2Games);
			await SoccerDataAccessor.PutReportAsync(method2Games);
			await NotificationService.PushMessagesAsync(method2Games);

			var method3Games = await SoccerService.GetMethod3GamesAsync(inPlayGammes);
			context.Logger.LogLine(string.Format("method3 game is {0}", method3Games.Count));
			await SoccerDataAccessor.PutGamesAsync(method3Games);
			await SoccerDataAccessor.PutReportAsync(method3Games);
			await NotificationService.PushMessagesAsync(method3Games);

			context.Logger.LogLine("SoccerDataCollector end");

			return input;
		}
	}
}