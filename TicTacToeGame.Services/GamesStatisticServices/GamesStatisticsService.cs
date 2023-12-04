using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GamesStatisticServices
{
    public class GamesStatisticsService : IGamesStatisticsService
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
        public Game GenerateRandomGame()
        {
            Random random = new Random();
            PlayerType[] playerTypes = { PlayerType.None, PlayerType.Host, PlayerType.Guest };
            GameState[] gameStates = { GameState.Declined, GameState.Starting, GameState.InProgress, GameState.Finished };

            Game game = new Game
            {
                Winner = playerTypes[random.Next(1, 3)],
                GameResult =  GameState.Finished,
                CurrentTurn = playerTypes[random.Next(1, 3)],

                PlayerHostId = "576cfe45-51b6-4392-bdae-3c9f88fdd946",
                PlayerGuestId = "0a38c567-e4b1-4ed3-b284-2bc4080851c6",

                GamesHistoryHostId = 1,
                GamesHistoryGuestId = 2,

                RoomId = 1
            };

            return game;
        }
        public async Task CreateTestGames()
        {
            List<Game> games = new List<Game>();
            for (int i = 0; i < 10; i++)
            {
                games.Add(GenerateRandomGame());
            }
            _gameRepository.AddEntities(games);
        }

        // Maybe refactor this method and return only GamesHistory with list of games
        public async Task<List<Game>> GetPlayedGames(string playerId)
        { // 1. we should know the player
          // 2. we should get id of user's games history
          // 3. we should get games by id of games history
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
        //public string GetDateOfGame(List<Game> games)
        //{
        //    return games.LastOrDefault().DateOfGame.ToString();
        //}
    }
}
