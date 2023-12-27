using System.Runtime.CompilerServices;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.HubConnections;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.Services.GameProcessService
{
    public class PlayerDisconectingTrackingService : IAsyncDisposable
    {
        private GameHubConnection _gameHubConnection;

        private readonly CheckForWinnerManager _checkForWinnerManager;

        private readonly GameManager _gameManager;

        private readonly GameReconnectingService _gameReconnectingService;

        public event Action StateHasChanged;

        public bool OpponentLeaved { get; set; } = false;
        public string LivedPlayerName { get; set; } = "";

        public bool IsOpponentConnected = false;

        public Timer _moveTimer;
        public Timer _responseTimer;


        public PlayerDisconectingTrackingService(GameManager gameManager, GameHubConnection gameHubConnection,
            CheckForWinnerManager checkForWinnerManager,
            GameReconnectingService gameReconnectingService)
        {
            _gameManager = gameManager;
            _gameHubConnection = gameHubConnection;
            _checkForWinnerManager = checkForWinnerManager;
            _gameReconnectingService = gameReconnectingService;
        }
        public void SetHubConnection(GameHubConnection gameHubConnection)
        {
            _gameHubConnection = gameHubConnection;
        }
        // Disconnection with timers
        public void InitializeTimers()
        {
            _moveTimer = new Timer(DisconnectingTrackingConstants.MOVE_TIME * 1000);
            _moveTimer.AutoReset = false;
            _moveTimer.Elapsed += async (sender, e) => await CheckIfPlayerAlive();

            _responseTimer = new Timer(DisconnectingTrackingConstants.RESPONSE_TIME * 1000);
            _responseTimer.AutoReset = false;
            _responseTimer.Elapsed += (sender, e) => OpponentIsNotAlive();

            //_moveTimer.Start();
        }
        public void StartTimers()
        {
            if (_moveTimer == null)
            {
                InitializeTimers();
                StartTimers();
            }
            else
            {
                _moveTimer.Start();
            }
        }

        private async Task CheckIfPlayerAlive()
        {
            if (_gameManager.CurrentGame == null)
            {
                _responseTimer?.Stop();
                _moveTimer?.Stop();
            }
            else
            {

                if (_gameHubConnection._hubConnection.State == Microsoft.AspNetCore.SignalR.Client.HubConnectionState.Disconnected)
                {
                    _responseTimer?.Stop();
                    _moveTimer?.Stop();
                }
                else
                {
                    await _gameHubConnection.CheckIfOpponentLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);

                    _responseTimer.Start();
                }
            }
        }
        // method that makes all dirty work
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

                await _gameHubConnection.SendOpponentLeft((int)_gameManager.CurrentGame.RoomId);
            }

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerHost.Id);

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerGuest.Id);
            
            //// Hz
            //_gameManager.ClearData();
            ////
           
            StateHasChanged?.Invoke();
        }

        public void ReloadMoveTimer()
        {
            _responseTimer?.Stop();
            _moveTimer?.Stop();
            _moveTimer?.Start();
        }

        // I think it`s on signalR
        public void ReceiveOpponentLeaves(int roomId, string userId)
        {
            if (_gameManager.CurrentPlayer != null && _gameManager.CurrentPlayer.Id != userId)
            {
                LivedPlayerName = _gameManager.GetOpponentName(userId);

                OpponentIsNotAlive();

                StateHasChanged?.Invoke();
            }
        }
        public void ReceiveCheckIfOpponentLeaves(int roomId, string userId)
        {
            CheckIfOpponentLeavesAsync(roomId, userId).GetAwaiter().GetResult();
        }
        public async Task CheckIfOpponentLeavesAsync(int roomId, string userId)
        {
            if (_gameManager.CurrentPlayer.Id != userId)
            {
                await _gameHubConnection.SendOpponentNotLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
            }
        }

        public void ReceiveOpponentNotLeaves(int roomId, string userId)
        {
            if (_gameManager.CurrentPlayer.Id != userId)
            {
                ReloadMoveTimer();
            }
        }

        // Disconnection on Leave Game button
        public void UpdateGameResultAfterTwoPlayersDisconnection()
        {
            if (_gameManager.CurrentPlayerGuest is not null && _gameManager.CurrentPlayerHost is not null)
            {
                Player host = _gameManager.PlayerRepository.GetById(_gameManager.CurrentPlayerHost.Id);
                Player guest = _gameManager.PlayerRepository.GetById(_gameManager.CurrentPlayerGuest.Id);

                if (!host.IsPlaying && !guest.IsPlaying)
                {
                    Game CurrentGame = _gameManager.GameRepository.GetById(_gameManager.CurrentGame.Id);

                    if (CurrentGame.GameResult != GameState.Declined)
                    {
                        CurrentGame.GameResult = GameState.Finished;
                        if (!_checkForWinnerManager.CheckForWinner())
                        {
                            if (_checkForWinnerManager.CheckForTie())
                            {
                                CurrentGame.Winner = PlayerType.None;
                            }
                        }
                        else
                        {
                            CurrentGame.Winner = (CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
                        }
                    }
                    _gameManager.GameRepository.UpdateEntity(CurrentGame);
                }
            }
        }
        // 
        public void ReceiveOpponentLeft()
        {
            OpponentLeaved = true;
            _checkForWinnerManager.GameStatus = "Game Over";
            StateHasChanged?.Invoke();
        }

        // Player connection

        // плеєр конектиться і чекає другого, другий конектиться
        // і відправляє меседж про це і коли той отримує то він перевіряє чи отримував вже чи ні якщо
        // ні то відправляє назад, цей отримує і
        // відправляє назад і той замикає коло бо не відправляє назад

        public bool isReceivedConnectedStatus = false;
        public async Task SendConnectedStatus(int roomId, string userId)
        {
            await _gameHubConnection.SendConnectedStatus(roomId, userId, isReceivedConnectedStatus);
        }
        public void ReceiveConnectedStatus(string userId, bool isAnotherPlayerNotified)
        {
            ReceiveConnectedStatusAsync(userId, isAnotherPlayerNotified).GetAwaiter().GetResult();
        }
        public async Task ReceiveConnectedStatusAsync(string userId, bool isAnotherPlayerNotified)
        {
            if (_gameManager.CurrentPlayer.Id != userId && !_gameManager.IsLoadingNextGame)
            {
                if (isReceivedConnectedStatus == false)
                {
                    isReceivedConnectedStatus = true;

                    StartTimers();

                    IsOpponentConnected = true;
                    StateHasChanged?.Invoke();
                }

                if (!isAnotherPlayerNotified)
                {
                    await _gameHubConnection.SendConnectedStatus((int)_gameManager.CurrentGame.RoomId, _gameManager.GetCurrentUserId(), isReceivedConnectedStatus);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _moveTimer?.Stop();
            _responseTimer?.Stop();

            _moveTimer?.Dispose();
            _responseTimer?.Dispose();

            await _gameHubConnection.DisposeAsync();
        }
    }
}
