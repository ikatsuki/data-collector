using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model;

namespace SoccerDataCollector
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
				.ToList();

			var home = common
				.Where(g => g.HomeOdds < g.AwayOdds / 6)
				.Where(g => g.HomeDangerousAttacks >= g.AwayDangerousAttacks * 1.8)
				.Where(g => g.HomeAttacks >= g.AwayAttacks)
				.Where(g => g.HomeOnTarget >= g.AwayOnTarget && g.HomeOnTarget >= 1)
				.Where(g => g.HomeOffTarget >= g.AwayOffTarget && g.HomeOffTarget >= 2)
				.ToList();

			var away = common
				.Where(g => g.AwayOdds < g.HomeOdds / 6)
				.Where(g => g.AwayDangerousAttacks >= g.HomeDangerousAttacks * 1.8)
				.Where(g => g.AwayAttacks >= g.HomeAttacks)
				.Where(g => g.AwayOnTarget >= g.HomeOnTarget && g.AwayOnTarget >= 1)
				.Where(g => g.AwayOffTarget >= g.HomeOffTarget && g.AwayOffTarget >= 2)
				.ToList();

			var targetGames = home.Concat(away).ToList();
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
				.Where(g => g.Time >= 70 && g.Time <= 80)
				.ToList();

			var home = common
				.Where(g => g.HomeDangerousAttacks >= g.AwayDangerousAttacks)
				.Where(g => g.HomeAttacks > g.AwayAttacks)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget > g.AwayOnTarget + g.AwayOffTarget)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= 8)
				.ToList();

			var away = common
				.Where(g => g.AwayDangerousAttacks >= g.HomeDangerousAttacks)
				.Where(g => g.AwayAttacks > g.HomeAttacks)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget > g.HomeOnTarget + g.HomeOffTarget)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= 8)
				.ToList();

			var targetGames = home.Concat(away).ToList();
			foreach (var game in home.Concat(away))
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
				.Where(g => g.HomeOdds < g.AwayOdds / 7)
				.Where(g => g.HomeDangerousAttacks >= g.AwayDangerousAttacks * 1.8)
				.Where(g => g.HomeAttacks >= g.AwayAttacks)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget > g.AwayOnTarget + g.AwayOffTarget)
				.Where(g => g.HomeOnTarget + g.HomeOffTarget >= 4)
				.ToList();

			var away = common
				.Where(g => g.AwayOdds < g.HomeOdds / 7)
				.Where(g => g.AwayDangerousAttacks >= g.HomeDangerousAttacks * 1.8)
				.Where(g => g.AwayAttacks >= g.HomeAttacks)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget > g.HomeOnTarget + g.HomeOffTarget)
				.Where(g => g.AwayOnTarget + g.AwayOffTarget >= 4)
				.ToList();

			var targetGames = home.Concat(away).ToList();
			foreach (var game in home.Concat(away))
			{
				var savedGameId = await SoccerDataAccessor.GetGameId(game.Id);
				targetGames.RemoveAll(g => g.Id == savedGameId);
			}

			return targetGames;
		}
	}
}