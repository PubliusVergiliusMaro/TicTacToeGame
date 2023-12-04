using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Claims;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.Services.GameProcessService
{
    public class PlayerDisconectingTrackingService
    {
        private HubConnection _hubConnection;
        private readonly PlayerRepository _playerRepository;
        private readonly GameRepository _gameRepository;

        // SignalR Events
        public event Action UpdateComponent;
        public bool OpponentLeaved { get; set; } = false;
        public string LivedPlayerName { get; set; } = "";// maybe remove and take from player

        // Timers
        public Timer _moveTimer;
        public Timer _responseTimer;

        // Players
        public Player CurrentPlayerHost;
        public Player CurrentPlayerGuest;
        public Player CurrentPlayer;

        // Game
        public Game CurrentGame;

        public PlayerDisconectingTrackingService(PlayerRepository playerRepository, GameRepository gameRepository)
        {
            _playerRepository = playerRepository;
            _gameRepository = gameRepository;
        }

        public void InitializeGame(Game game)
        {
            CurrentGame = game;
        }

        public void InitizalisePlayers(Player playerHost, Player playerGuest, ClaimsPrincipal user)
        {
            CurrentPlayerHost = playerHost;
            CurrentPlayerGuest = playerGuest;
            CurrentPlayer = _playerRepository.GetCurrentPlayer(CurrentPlayerHost, CurrentPlayerGuest, user);
        }
        public void SetHubConnection(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;

            hubConnection.On<string>("UserDisconnected", (connectionId) =>
                UserDisconnected(connectionId));

            hubConnection.On<Guid, string>("OpponentLeaves", (gameId, connectionId) =>
                OpponentLeaves(gameId, connectionId));

            hubConnection.On<Guid, string>("CheckIfOpponentLeaves", (gameId, connectionId) =>
                CheckIfOpponentLeaves(gameId, connectionId));

            hubConnection.On<Guid, string>("OpponentNotLeaves", (gameId, connectionId) =>
                OpponentNotLeaves(gameId, connectionId));
        }
        public void InitializeTimers()
        {
            _moveTimer = new Timer(10_000);// set more time
            _moveTimer.AutoReset = false;
            _moveTimer.Elapsed += async (sender, e) => await CheckIfPlayerAlive();

            _responseTimer = new Timer(10_000);
            _responseTimer.AutoReset = false;
            _responseTimer.Elapsed += (sender, e) => OpponentIsNotAlive();

            _moveTimer.Start();
        }

        private async Task CheckIfPlayerAlive()
        {
            await _hubConnection.SendAsync("CheckIfOpponentLeaves", CurrentGame.UniqueId, CurrentPlayer.GameConnectionId);
            _responseTimer.Start();
        }

        private void OpponentIsNotAlive()
        {
            OpponentLeaved = true;
            _moveTimer.Stop();
            _responseTimer.Stop();

            if (CurrentGame.GameResult != GameState.Finished)
            {
                CurrentGame.GameResult = GameState.Finished;
                CurrentGame.Winner = PlayerType.None;

                _gameRepository.UpdateEntity(CurrentGame);
            }
        }
        public string GetOpponentName(string connectionId)
        {
            if (CurrentPlayerHost.GameConnectionId == connectionId)
            {
                return CurrentPlayerHost.UserName;
            }
            else
            {
                return CurrentPlayerGuest.UserName;
            }
        }
        public void ReloadMoveTimer()
        {
            _responseTimer.Stop();
            _moveTimer.Stop();
            _moveTimer.Start();
        }

        private void UserDisconnected(string connectionId)
        {
            if (CurrentPlayerHost.GameConnectionId == connectionId ||
                CurrentPlayerGuest.GameConnectionId == connectionId)
            {
                LivedPlayerName = GetOpponentName(connectionId);

                OpponentIsNotAlive();

                UpdateComponent?.Invoke();
            }
        }
        private void OpponentLeaves(Guid gameId, string connectionId)
        {
            if (CurrentGame.UniqueId == gameId)
            {
                LivedPlayerName = GetOpponentName(connectionId);

                OpponentIsNotAlive();

                UpdateComponent?.Invoke();
            }
        }
        private async Task CheckIfOpponentLeaves(Guid gameId, string connectionId)
        {
            if (CurrentPlayer.GameConnectionId != connectionId)
            {
                await _hubConnection.SendAsync("OpponentNotLeaves", CurrentGame.UniqueId, CurrentPlayer.GameConnectionId);
            }
        }

        private void OpponentNotLeaves(Guid gameId, string connectionId)
        {
            if (CurrentPlayer.GameConnectionId != connectionId)
            {
                _responseTimer.Stop();
                _moveTimer.Stop();
                _moveTimer.Start();
            }
        }
    }
}
