using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Claims;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.HubConnections;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.Services.GameProcessService
{
    public class PlayerDisconectingTrackingService
    {
        private readonly GameHubConnection _gameHubConnection;

        private readonly GameManager _gameManager;

        public event Action UpdateComponent;
        // SignalR Events
        public bool OpponentLeaved { get; set; } = false;
        public string LivedPlayerName { get; set; } = "";

        // Timers
        public Timer _moveTimer;
        public Timer _responseTimer;

      
        public PlayerDisconectingTrackingService(GameManager gameManager, GameHubConnection gameHubConnection)
        {
            _gameManager = gameManager;
            _gameHubConnection = gameHubConnection;
        }

        public void InitializeTimers()
        {
            _moveTimer = new Timer(DisconnectingTrackingConstants.MOVE_TIME * 1000);
            _moveTimer.AutoReset = false;
            _moveTimer.Elapsed += async (sender, e) => await CheckIfPlayerAlive();

            _responseTimer = new Timer(DisconnectingTrackingConstants.RESPONSE_TIME * 1000);
            _responseTimer.AutoReset = false;
            _responseTimer.Elapsed += (sender, e) => OpponentIsNotAlive();

            _moveTimer.Start();
        }

        private async Task CheckIfPlayerAlive()
        {
            await _gameHubConnection.CheckIfOpponentLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.GameConnectionId);
            _responseTimer.Start();
        }

        private async void OpponentIsNotAlive()
        {
            OpponentLeaved = true;
            _moveTimer.Stop();
            _responseTimer.Stop();

            if (_gameManager.CurrentGame.GameResult != GameState.Finished)
            {
                _gameManager.CurrentGame.GameResult = GameState.Declined;
                _gameManager.CurrentGame.Winner = PlayerType.None;

                _gameManager.GameRepository.UpdateEntity(_gameManager.CurrentGame);

                await _gameHubConnection.OpponentLeft((int)_gameManager.CurrentGame.RoomId);
            }
        }
       
        public void ReloadMoveTimer()
        {
            _responseTimer?.Stop();
            _moveTimer?.Stop();
            _moveTimer?.Start();
        }

        public void OpponentLeaves(int roomId, string connectionId)
        {
            if (_gameManager.CurrentGame.RoomId == roomId)
            {
                LivedPlayerName = _gameManager.GetOpponentName(connectionId);

                OpponentIsNotAlive();

                UpdateComponent?.Invoke();
            }
        }
        public void CheckIfOpponentLeaves(int roomId, string connectionId)
        {
            CheckIfOpponentLeavesAsync(roomId, connectionId).GetAwaiter().GetResult();
        }
        public async Task CheckIfOpponentLeavesAsync(int roomId, string connectionId)
        {
            if (_gameManager.CurrentPlayer.GameConnectionId != connectionId)
            {
                await _gameHubConnection.OpponentNotLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.GameConnectionId);
            }
        }

        public void OpponentNotLeaves(int roomId, string connectionId)
        {
            if (_gameManager.CurrentPlayer.GameConnectionId != connectionId)
            {
                ReloadMoveTimer();
            }
        }
    }
}
