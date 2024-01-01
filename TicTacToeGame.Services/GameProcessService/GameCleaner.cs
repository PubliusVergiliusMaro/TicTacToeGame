using Microsoft.Extensions.Logging;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameCleaner
    {
        private readonly GameRepository _gameRepository;

        private readonly ILogger<GameCleaner> _logger;  

        public GameCleaner(GameRepository gameRepository, ILogger<GameCleaner> logger)
        {
            _gameRepository = gameRepository;
            _logger = logger;
        }

        public void ClearEmptyGames(string playerId)
        {
            List<Game> emptyGames = _gameRepository.GetOldNotClosedGamesByPlayerId(playerId);

            foreach (var emptyGame in emptyGames)
            {
                _gameRepository.UpdateGameResult(emptyGame.Id, GameState.Declined);
            }
            _logger.LogError($"Empty games cleared for player {playerId}. Updated games count: {emptyGames.Count}");
        }
    }
}
