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
            if(gamesHistory == null)
            {  // handle null
                // maybe send message to user that error occured
                throw new Exception("Games history not found");
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
                Winner = playerTypes[random.Next(1,3)],
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
            for(int i = 0; i < 10; i++)
            {
                games.Add(GenerateRandomGame());
            }
            _gameRepository.AddEntities(games);
        }

        // Maybe refactor this method and return only GamesHistory with list of games
        public async Task<List<Game>> GetPlayedGames(string playerId)
        {
            // 1. we should know the player
            // 2. we should get id of user's games history
            // 3. we should get games by id of games history
            GamesHistory gamesHistory = await GetGamesHistoryByPlayerId(playerId);
            
            List<Game>? games = _gameRepository.GetGamesByGamesHistoryId(gamesHistory.Id);
            if(games == null)
            {
                // handle null
                // maybe send message to user that error occured
                throw new Exception("Games not found");
            }

            return games;
        }
        public Player GetPlayerById(string playerId)
        {
            Player player = _playerRepository.GetById(playerId);
            if(player == null)
            {
                // handle null
                // maybe send message to user that error occured
                throw new Exception("Player not found");
            }
            return player;
        }
    }
}
