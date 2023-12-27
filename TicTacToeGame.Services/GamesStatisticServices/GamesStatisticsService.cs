using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;

namespace TicTacToeGame.Services.GamesStatisticServices
{
    public class GamesStatisticsService
    {
        private readonly GamesHistoryRepository _gamesHistoryRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly GameRepository _gameRepository;

        public GamesStatisticsService(GamesHistoryRepository gamesHistoryRepository,
            PlayerRepository playerRepository,
            GameRepository gameRepository)
        {
            _gamesHistoryRepository = gamesHistoryRepository;
            _playerRepository = playerRepository;
            _gameRepository = gameRepository;
        }
        public async Task<GamesHistory> GetGamesHistoryByPlayerId(string playerId)
        {
            GamesHistory gamesHistory = await _gamesHistoryRepository.GetGamesHistoryByPlayerId(playerId);

            if (gamesHistory == null)
            {
                return null;
            }

            return gamesHistory;
        }

        public async Task<List<Game>> GetPlayedGames(string playerId)
        {
            try
            {
                // 1. Get the player's game history
                GamesHistory gamesHistory = await GetGamesHistoryByPlayerId(playerId);

                if (gamesHistory == null)
                {
                    throw new ArgumentNullException(nameof(gamesHistory), "Games history not found for the player");
                }

                // 2. Get games by the id of the games history
                List<Game> games = _gameRepository.GetGamesByGamesHistoryId(gamesHistory.Id);

                if (games == null)
                {
                    throw new InvalidOperationException("Games not found for the player");
                }

                return games;
            }
            catch (ArgumentNullException ex)
            {
                // Log the ArgumentNullException
                Console.WriteLine($"ArgumentNullException in GetPlayedGames: {ex.Message}");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                // Log the InvalidOperationException
                Console.WriteLine($"InvalidOperationException in GetPlayedGames: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Log any other exceptions for further analysis
                Console.WriteLine($"Exception in GetPlayedGames: {ex.Message}");
                return null;
            }
        }

        public Player GetWinner(PlayerType? playerType, Player hostPlayer, Player guestPlayer)
        {
            switch (playerType)
            {
                case PlayerType.Host:
                    return hostPlayer;
                case PlayerType.Guest:
                    return guestPlayer;
                default:
                    return null;
            }
        }
        public Player GetPlayerById(string playerId)
        {
            Player player = _playerRepository.GetById(playerId);
            if (player == null)
            {
                return null;
            }
            return player;
        }
        public void GetGamesByResultAndPlayers(GameManager gameManager)
        {
            gameManager.GamesHistory.Games = _gameRepository.GetGameHistoryInSession(GameState.Finished, gameManager.CurrentPlayerHost.Id,
              gameManager.  CurrentPlayerGuest.Id, gameManager. CurrentGame.RoomId);
            gameManager. HostWins = gameManager. GamesHistory.Games.Count(game => game.Winner == PlayerType.Host);
            gameManager.GuestWins = gameManager. GamesHistory.Games.Count(game => game.Winner == PlayerType.Guest);
        }
    }
}
