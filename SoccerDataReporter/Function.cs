using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Core;
using CsvHelper;

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
		public async Task<dynamic> FunctionHandler(Event input, ILambdaContext context)
		{
			if (!DateTime.TryParse(input.Time, out DateTime inputDt))
				return input;

			context.Logger.LogLine($"SoccerDataReporter start at {DateTime.Now:yyyy-MM-dd HH:mm}");
			var reportDate = $"{inputDt.AddDays(-1):yyyyMMdd}";
			context.Logger.LogLine($"reportDate: {reportDate}");

			var reports = await SoccerDataAccessor.GetGamesForReportAsync(reportDate);
			foreach (var report in reports)
			{
				report.Events = await ScrapeService.GetGameEventsAsync(report.DetailUrl);
				report.Win = SoccerService.GetMethodResult(report);
				report.GoalTimes = report.Events == null ? null : string.Join("-", report.Events.Select(e => e.GoalTime));
			}
			
			context.Logger.LogLine($"report count: {reports.Count}");

			var reportMessage = await NotificationService.PushDailyReportAsync(reports);

			var fileName = $"soccer-report-{reportDate}.csv";
			var filePath = $@"/tmp/{fileName}";

			using (var textWriter = File.CreateText(filePath))
			{
				var csv = new CsvWriter(textWriter);
				csv.WriteRecords(reports);
			}

			var response = await SoccerDataAccessor.UploadCsvFile(filePath);
			context.Logger.LogLine($"upload csv file to s3. status = {response.HttpStatusCode}");

			context.Logger.LogLine("SoccerDataReporter end");

			return reportMessage;
		}
	}
}