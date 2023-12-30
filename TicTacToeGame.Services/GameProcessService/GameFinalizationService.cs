using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameFinalizationService
    {
        private readonly GameManager _gameManager;

        private readonly GameRepository _gameRepository;

        private readonly GameHubConnection _gameHubConnection;

        private readonly GamesStatisticsService _gamesStatisticsService;

        private readonly GameReconnectingService _gameReconnectingService;

        private readonly GameBoardManager _gameBoardManager;

        public GameFinalizationService(GameManager gameManager,
            GameRepository gameRepository,
            GameHubConnection gameHubConnection,
            GameBoardManager gameBoardManager)
        {
            _gameManager = gameManager;
            _gameRepository = gameRepository;
            _gameBoardManager = gameBoardManager;
            _gameHubConnection = gameHubConnection;
        }

        public async Task FinishGame( string gameState, bool isTie = false)
        {
            _gameManager.CurrentGame.GameResult = GameState.Finished;

            GamesHistory hostGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(_gameManager.CurrentPlayerHost.Id);
            GamesHistory guestGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(_gameManager.CurrentPlayerGuest.Id);

            _gameManager.CurrentGame.GamesHistoryHostId = hostGamesHistory.Id;
            _gameManager.CurrentGame.GamesHistoryGuestId = guestGamesHistory.Id;
            
            if (isTie)
            {
                _gameManager.CurrentGame.Winner = PlayerType.None;
            }
            else
                _gameManager.CurrentGame.Winner = (_gameManager.CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;

            _gameManager.GameRepository.UpdateEntity(_gameManager.CurrentGame);
            
            await SendGameStatus(gameState);

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerHost.Id);
            _gameManager.CurrentPlayerHost.IsPlaying = false;
            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerGuest.Id);
            _gameManager.CurrentPlayerGuest.IsPlaying = false;

            //_gamesStatisticsService.UpdatePlayersGameHistory(_gameManager.CurrentPlayerHost.Id, _gameManager.CurrentPlayerGuest.Id, _gameManager.CurrentGame.RoomId);

            _gameBoardManager.RemoveBoard(_gameManager.CurrentGame.UniqueId);

            //StateHasChanged?.Invoke();
        }
        private async Task SendGameStatus(string GameStatus)
        {
            await _gameHubConnection.SendGameStatus(_gameManager.CurrentGame.GameResult, GameStatus, (int)_gameManager.CurrentGame.RoomId);
        }
        //TODO: implement
        //      1. Method that will send to all clients message that the game is finished
        //      2. method that will make game finished and add all required data to the database
    }
}
