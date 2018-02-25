using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SoccerDataReporter
{
	public class Function
	{
		private static ScrapeService ScrapeService => new ScrapeService();

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
			var now = DateTime.Now.AddDays(-1);
			context.Logger.LogLine($"SoccerDataReporter start at {now:yyyy-MM-dd HH:mm}");
			context.Logger.LogLine($"input: {input}");

			var reportDate = $"{now:yyyyMMdd}";
			var reports = await SoccerDataAccessor.GetGamesForReportAsync(reportDate);
			foreach (var report in reports)
			{
				report.Events = await ScrapeService.GetGameEventsAsync(report);

			}
			
			context.Logger.LogLine($"report count: {reports.Count}");

			var reportMessage = await NotificationService.PushMessagesAsync(reports, context);

			context.Logger.LogLine("SoccerDataReporter end");

			return reportMessage;
		}
	}
}