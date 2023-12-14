using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Claims;
using TicTacToeGame.Domain.Constants;
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
        public string LivedPlayerName { get; set; } = "";

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

            hubConnection.On<int, string>("OpponentLeaves", (gameId, connectionId) =>
                OpponentLeaves(gameId, connectionId));

            hubConnection.On<int, string>("CheckIfOpponentLeaves", (roomId, connectionId) =>
                CheckIfOpponentLeaves(roomId, connectionId));

            hubConnection.On<int, string>("OpponentNotLeaves", (roomId, connectionId) =>
                OpponentNotLeaves(roomId, connectionId));
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
            await _hubConnection.SendAsync("CheckIfOpponentLeaves", CurrentGame.RoomId, CurrentPlayer.GameConnectionId);
            _responseTimer.Start();
        }

        private async void OpponentIsNotAlive()
        {
            OpponentLeaved = true;
            _moveTimer.Stop();
            _responseTimer.Stop();

            if (CurrentGame.GameResult != GameState.Finished)
            {
                CurrentGame.GameResult = GameState.Finished;
                CurrentGame.Winner = PlayerType.None;
                
                _gameRepository.UpdateEntity(CurrentGame);
                
                await _hubConnection.SendAsync("OpponentLeft", CurrentGame.RoomId);
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

        private void OpponentLeaves(int roomId, string connectionId)
        {
            if (CurrentGame.RoomId == roomId)
            {
                LivedPlayerName = GetOpponentName(connectionId);

                OpponentIsNotAlive();

                UpdateComponent?.Invoke();
            }
        }
        private async Task CheckIfOpponentLeaves(int roomId, string connectionId)
        {
            if (CurrentPlayer.GameConnectionId != connectionId)
            {
                await _hubConnection.SendAsync("OpponentNotLeaves", CurrentGame.RoomId, CurrentPlayer.GameConnectionId);
            }
        }

        private void OpponentNotLeaves(int roomId, string connectionId)
        {
            if (CurrentPlayer.GameConnectionId != connectionId)
            {
                ReloadMoveTimer();
            }
        }

        public async Task<bool> CheckIfTwoPlayersArePlaying()
        {
           return await _playerRepository.CheckIfTwoPlayersArePlaying(CurrentPlayerHost.Id, CurrentPlayerGuest.Id);
        }
    }
}
