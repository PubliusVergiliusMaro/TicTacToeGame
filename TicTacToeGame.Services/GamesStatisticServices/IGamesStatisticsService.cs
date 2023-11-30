using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.GamesStatisticServices
{
    public interface IGamesStatisticsService
    {
        Task<GamesHistory> GetGamesHistoryByPlayerId(string playerId);
        Game GenerateRandomGame();
        Task CreateTestGames();
        Task<List<Game>> GetPlayedGames(string playerId);
        Player GetPlayerById(string playerId);
        Player GetWinner(PlayerType? playerType, Player hostPlayer, Player guestPlayer);
    }
}
