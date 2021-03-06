﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace Core
{
	public class SoccerService
	{
		private static SoccerDataAccessor SoccerDataAccessor => new SoccerDataAccessor();

		/// <summary>
		///     Over0.5手法
		/// </summary>
		public async Task<IList<Game>> GetMethod1GamesAsync(IEnumerable<Game> games)
		{
			var common = games
				.Where(g => g.HomeScore + g.AwayScore == 0)
				.Where(g => g.Time >= 18 && g.Time <= 35)
				.Where(g => g.Category.IndexOf("women", StringComparison.OrdinalIgnoreCase) < 0)
				.ToList();

			var home = common
				.Where(g => g.HomeOdds < g.AwayOdds / 6)
				.Where(g => g.HomeDangerousAttacks >= g.AwayDangerousAttacks * 1.8)
				.Where(g => g.HomeAttacks >= g.AwayAttacks)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= g.AwayOnTarget + g.AwayOffTarget)
				.Where(g => g.HomeOnTarget >= 1)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= 3)
				.ToList();

			var away = common
				.Where(g => g.AwayOdds < g.HomeOdds / 6)
				.Where(g => g.AwayDangerousAttacks >= g.HomeDangerousAttacks * 1.8)
				.Where(g => g.AwayAttacks >= g.HomeAttacks)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= g.HomeOnTarget + g.HomeOffTarget)
				.Where(g => g.AwayOnTarget >= 1)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= 3)
				.ToList();

			var targetGames = home.Concat(away).ToList();
			SetMethodNo(1, ref targetGames);

			foreach (var game in home.Concat(away))
			{
				var savedGameId = await SoccerDataAccessor.GetGameId(game.Id);
				targetGames.RemoveAll(g => g.Id == savedGameId);
			}

			return targetGames;
		}

		/// <summary>
		///     大量得点手法
		/// </summary>
		public async Task<IList<Game>> GetMethod2GamesAsync(IEnumerable<Game> games)
		{
			var common = games
				.Where(g => Math.Abs(g.HomeScore - g.AwayScore) >= 4)
				.Where(g => g.Time >= 73 && g.Time <= 80)
				.Where(g => g.Events != null && g.Events.Any(e => e.GoalTime > 45 && e.GoalTime < 80))
				.ToList();

			var home = common
				.Where(g => g.HomeDangerousAttacks > g.AwayDangerousAttacks)
				.Where(g => g.HomeAttacks > g.AwayAttacks)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget > g.AwayOnTarget + g.AwayOffTarget)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= 8)
				.ToList();

			var away = common
				.Where(g => g.AwayDangerousAttacks > g.HomeDangerousAttacks)
				.Where(g => g.AwayAttacks > g.HomeAttacks)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget > g.HomeOnTarget + g.HomeOffTarget)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= 8)
				.ToList();

			var targetGames = home.Concat(away).ToList();
			SetMethodNo(2, ref targetGames);

			foreach (var game in targetGames)
			{
				var savedGameId = await SoccerDataAccessor.GetGameId(game.Id);
				targetGames.RemoveAll(g => g.Id == savedGameId);
			}

			return targetGames;
		}

		/// <summary>
		///     前半大量得点手法
		/// </summary>
		public async Task<IList<Game>> GetMethod3GamesAsync(IEnumerable<Game> games)
		{
			var common = games
				.Where(g => Math.Abs(g.HomeScore - g.AwayScore) >= 1)
				.Where(g => g.Time >= 15 && g.Time <= 25)
				.ToList();

			var home = common
				.Where(g => g.HomeOdds == default(decimal) ||
				            g.HomeOdds < g.AwayOdds / (8 * (Math.Abs(g.HomeScore - g.AwayScore) + 1)))
				.Where(g => g.HomeDangerousAttacks >= g.AwayDangerousAttacks * 1.5)
				.Where(g => g.HomeAttacks > g.AwayAttacks)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget > g.AwayOnTarget + g.AwayOffTarget)
				.Where(g => g.HomeOnTarget >= 2)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= 4)
				.ToList();

			var away = common
				.Where(g => g.AwayOdds == default(decimal) ||
				            g.AwayOdds < g.HomeOdds / (8 * (Math.Abs(g.HomeScore - g.AwayScore) + 1)))
				.Where(g => g.AwayDangerousAttacks >= g.HomeDangerousAttacks * 1.5)
				.Where(g => g.AwayAttacks > g.HomeAttacks)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget > g.HomeOnTarget + g.HomeOffTarget)
				.Where(g => g.AwayOnTarget >= 2)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= 4)
				.ToList();

			var targetGames = home.Concat(away).ToList();
			SetMethodNo(3, ref targetGames);

			foreach (var game in home.Concat(away))
			{
				var savedGameId = await SoccerDataAccessor.GetGameId(game.Id);
				targetGames.RemoveAll(g => g.Id == savedGameId);
			}

			return targetGames;
		}

		private static void SetMethodNo(int methodNo, ref List<Game> games)
		{
			foreach (var targetGame in games)
			{
				targetGame.Id = $"{targetGame.Id}${methodNo}";
				targetGame.Method = methodNo;
			}
		}

		public static bool? GetMethodResult(Report report)
		{
			if (report.Events == null)
				return null;

			switch (report.Method)
			{
				case 1:
				case 3:
					return GetHalfTimeResult(report);
				case 2:
					return GetFullTimeResult(report);
			}

			return false;
		}

		private static bool GetHalfTimeResult(Report report)
		{
			return report.Events.Any(e => e.GoalTime > report.Time && e.GoalTime <= 45);
		}

		private static bool GetFullTimeResult(Report report)
		{
			return report.Events.Any(e => e.GoalTime > report.Time && e.GoalTime <= 90);
		}
	}
}