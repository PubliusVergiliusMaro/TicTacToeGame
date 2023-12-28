using Microsoft.AspNetCore.Components;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameSessionService
    {
        private GameHubConnection _gameHubConnection;

        private readonly NavigationManager _navigationManager;

        private readonly GameManager _gameManager;

        public event Action StateHasChanged;

        public bool RequestForNextGame { get; set; } = false;
        public bool IsTwoPlayersPlaying { get; set; } = false;
        public bool ApprovedNextGame { get; set; } = false;
        public bool DeclinedNextGame { get; set; } = false;
        public bool SendedRequestForNextGame { get; set; } = false;

        public GameSessionService(NavigationManager navigationManager,
            GameHubConnection gameHubConnection,
            GameManager gameManager)
        {
            _navigationManager = navigationManager;
            _gameHubConnection = gameHubConnection;
            _gameManager=gameManager;
        }
        public void SetHubConnection(GameHubConnection gameHubConnection)
        {
            _gameHubConnection = gameHubConnection;
        }
       
        public async Task SendRequestOnNextGame(int roomId, string userId)
        {
            SendedRequestForNextGame = true;
            await _gameHubConnection.AskAnotherPlayerForNextGame(roomId, userId);
        }

        public void ReceiveRequestOnNextGame(string userId)
        {
            if (userId != _gameManager.CurrentPlayer.Id)
            {
                if (SendedRequestForNextGame == false)
                {
                    RequestForNextGame = true;
                    StateHasChanged?.Invoke();
                }
            }
        }

        // Decline request on next game
        public async Task DeclineRequestOnNextGame()
        {
            await _gameHubConnection.SendDeclineAnotherGameRequest((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);

            DeclinedNextGame = true;
            StateHasChanged?.Invoke();
        }
        public void ReceiveDeclineRequestOnNextGame(string userId)
        {
            ApprovedNextGame = false;
            DeclinedNextGame = true;
            RequestForNextGame = true;
            StateHasChanged?.Invoke();
        }

        // Accept request on next game
        public async Task AcceptNextGameRequest()
        {
            bool isSuccesfullyCreatedGame = await CreateNextGame();
        
            if (isSuccesfullyCreatedGame)
            {
                await _gameHubConnection.SendReadyNextGameStatus((int)_gameManager.CurrentGame.RoomId);
            }
        }
        public void ReceiveReadyNextGameStatus()
        {
            _gameManager.IsLoadingNextGame = true;

            _navigationManager.NavigateTo("/game", forceLoad: true);
        }

        public async Task<bool> CreateNextGame()
        {
            try
            {
                Game newGame = new()
                {
                    PlayerHostId = _gameManager.CurrentGame.PlayerHostId,
                    PlayerGuestId = _gameManager.CurrentGame.PlayerGuestId,
                    RoomId = _gameManager.CurrentGame.RoomId,
                    GameResult = GameState.Starting,
                    CurrentTurn = new Random().Next(3) == 0 ? PlayerType.Host : PlayerType.Guest
                };

                _gameManager.GameRepository.AddEntity(newGame);

                _gameManager.PlayerRepository.UpdatePlayerStatus(_gameManager.CurrentPlayer.Id, false);
                _gameManager.PlayerRepository.UpdatePlayerStatus(_gameManager.CurrentGame.PlayerHostId, false);
                _gameManager.PlayerRepository.UpdatePlayerStatus(_gameManager.CurrentGame.PlayerGuestId, false);

                _gameManager.CurrentPlayer.IsPlaying = false;
                _gameManager.CurrentPlayerHost.IsPlaying = false;
                _gameManager.CurrentPlayerGuest.IsPlaying = false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
