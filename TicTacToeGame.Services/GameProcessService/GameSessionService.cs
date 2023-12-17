using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameSessionService
    {
        private readonly GameHubConnection _gameHubConnection;

        private readonly NavigationManager _navigationManager;

        private readonly GameManager _gameManager;

        public event Action StateHasChanged;

        public bool RequestForNextGame { get; set; } = false;
        public bool IsTwoPlayersPlaying { get; set; } = false;
        public bool ApprovedNextGame { get; set; } = false;
        public bool DeclinedNextGame { get; set; } = false;

        public GameSessionService(NavigationManager navigationManager,
            GameHubConnection gameHubConnection,
            GameManager gameManager)
        {
            _navigationManager = navigationManager;
            _gameHubConnection = gameHubConnection;
            _gameManager=gameManager;
        }

        public void AskAnotherPlayerForNextGame(string userId)
        {
            if (userId != _gameManager.CurrentPlayer.Id)
            {
                RequestForNextGame = true;
                StateHasChanged?.Invoke();
            }
        }
        public void DeclineAnotherGameRequest(string userId)
        {
            ApprovedNextGame = false;
            DeclinedNextGame = true;
            RequestForNextGame = true;
            StateHasChanged?.Invoke();
        }
        public void AcceptAnotherGameRequest(string userId)
        {
            AcceptAnotherGameRequestAsync(userId).GetAwaiter().GetResult();
        }
        public async Task AcceptAnotherGameRequestAsync(string userId)
        {
            if (userId != _gameManager.CurrentPlayer.Id)
            {
                ApprovedNextGame = true;
                DeclinedNextGame = false;
                Random random = new Random();
                Game newGame = new()
                {
                    PlayerHostId = _gameManager.CurrentGame.PlayerHostId,
                    PlayerGuestId = _gameManager.CurrentGame.PlayerGuestId,
                    RoomId = _gameManager.CurrentGame.RoomId,
                    GameResult = GameState.Starting,
                    CurrentTurn = random.Next(3) == 0 ? PlayerType.Host : PlayerType.Guest
                };

                _gameManager.GameRepository.AddEntity(newGame);
                // make that host becomes such player that sends request
                _gameManager.PlayerRepository.UpdatePlayerStatus(_gameManager.CurrentPlayer.Id, true);
                // make new game with same players

                await _gameHubConnection.JoinNextGame((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);

                _navigationManager.NavigateTo("/game", forceLoad: true);
            }
        }
        public void JoinNextGame(string userId)
        {
            if (userId != _gameManager.CurrentPlayer.Id)
            {
                _gameManager.PlayerRepository.UpdatePlayerStatus(_gameManager.CurrentPlayer.Id, true);

                _navigationManager.NavigateTo("/game", forceLoad: true);
            }
        }

        public async Task ApproveNextGamePlayerRequest()
        {
            await _gameHubConnection.AcceptAnotherGameRequest((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
        }
        public async Task DeclineNextPlayerRequest()
        {
            await _gameHubConnection.DeclineAnotherGameRequest((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);

            DeclinedNextGame = true;
            StateHasChanged?.Invoke();
        }
        public async Task PlayNextGame()
        {
            await _gameHubConnection.AskAnotherPlayerForNextGame((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
        }
    }
}
