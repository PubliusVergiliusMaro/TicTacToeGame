using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
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

        private readonly GameBoardManager _gameBoardManager;

        private readonly ILogger<PlayerDisconectingTrackingService> _logger;

        public event Action StateHasChanged;

        public string LivedPlayerName { get; set; } = "";
        public bool OpponentLeaved { get; set; } = false;

        private int _moveTimerElapsedCounter = 0;

        public bool IsOpponentConnected = false;

        public bool IsWaitingTimeUp = false;

        public Timer _moveTimer;
        public Timer _responseTimer;
        public Timer _waitingAnotherPlayerTimer;

        public int WaitingTime = DisconnectingTrackingConstants.WAITING_ANOTHER_PLAYER_SECONDS;

        public PlayerDisconectingTrackingService(GameManager gameManager,
            GameHubConnection gameHubConnection,
            CheckForWinnerManager checkForWinnerManager,
            GameReconnectingService gameReconnectingService,
            ILogger<PlayerDisconectingTrackingService> logger,
            GameBoardManager gameBoardManager)
        {
            _logger = logger;
            _gameManager = gameManager;
            _gameBoardManager = gameBoardManager;
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

            _waitingAnotherPlayerTimer = new Timer(1000);
            _waitingAnotherPlayerTimer.AutoReset = true;
            _waitingAnotherPlayerTimer.Elapsed += (sender, e) =>
            {
                WaitingTime--;
                if (WaitingTime == 0)
                {
                    AnotherPlayerNotConnected();
                    _waitingAnotherPlayerTimer?.Stop();
                }
                StateHasChanged?.Invoke();
            };
            //_moveTimer.Start();
        }

        private void AnotherPlayerNotConnected()
        {
            // Close game

            OpponentIsNotAlive();

            IsWaitingTimeUp = true;
            StateHasChanged?.Invoke();
        }

        public void StartTimers()
        {
            if (_moveTimer == null)
            {
                InitializeTimers();
                _waitingAnotherPlayerTimer?.Stop();
                StartTimers();

                _logger.LogError("Timer starts");
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
                _logger.LogError("Timer stops");
            }
            else
            {
                if (_gameHubConnection._hubConnection.State == HubConnectionState.Disconnected)
                {
                    _responseTimer?.Stop();
                    _moveTimer?.Stop();
                    _logger.LogError("Timer stops");
                }
                else
                {
                    if (_moveTimerElapsedCounter < DisconnectingTrackingConstants.MOVE_TIMER_ITERATIONS)
                    {
                        _moveTimerElapsedCounter++;
                        _logger.LogError("Sending CheckIfOpponentLeaves. Move Timer elapsed count: {Count}",_moveTimerElapsedCounter);
                        await _gameHubConnection.CheckIfOpponentLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
                        _moveTimer?.Start();
                    }
                    else
                    {
                        _logger.LogError("Sending CheckIfOpponentLeaves. Sending last message");
                        await _gameHubConnection.CheckIfOpponentLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
                        _logger.LogError("Starting response timer");
                        _responseTimer?.Start();
                        _moveTimerElapsedCounter = 0;
                    }
                }
            }
        }
        // method that makes all dirty work
        private async void OpponentIsNotAlive()
        {
            OpponentLeaved = true;
            _moveTimer?.Stop();
            _responseTimer?.Stop();
            _logger.LogError("Opponent is not alive");
            if (_gameManager.CurrentGame != null && _gameManager.CurrentGame.GameResult != GameState.Finished)
            {
                _gameManager.CurrentGame.GameResult = GameState.Declined;
                _gameManager.CurrentGame.Winner = PlayerType.None;

                _gameManager.GameRepository.UpdateEntity(_gameManager.CurrentGame);

                await _gameHubConnection.SendUserLeftMessageToTheRoom((int)_gameManager.CurrentGame.RoomId);// Refactor and maybe remove
            }

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerHost.Id);

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerGuest.Id);

            _gameBoardManager.RemoveBoard(_gameManager.CurrentGame.UniqueId);

            StateHasChanged?.Invoke();
        }

        public void ReloadMoveTimer(string senderId)
        {
            if (_gameManager.CurrentPlayer != null && _gameManager.CurrentPlayer.Id != senderId)
            {
                _moveTimerElapsedCounter = 0;
                _responseTimer?.Stop();
                _moveTimer?.Stop();
                _moveTimer?.Start();
                _logger.LogError("Reloading move timers");
            }
        }
        public void StartWaitingAnotherPlayerTimer()
        {
            _waitingAnotherPlayerTimer?.Start();
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
                _logger.LogError("Receives check if i`m alive");
                await _gameHubConnection.SendOpponentNotLeaves((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id);
            }
        }

        public void ReceiveOpponentNotLeaves(int roomId, string userId)
        {
            if (_gameManager.CurrentPlayer.Id != userId)
            {
                _logger.LogError("Reeceive opponent not leaves");
                ReloadMoveTimer(userId);
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
                    _logger.LogError("Two players leaves");
                    if (CurrentGame.GameResult != GameState.Declined)
                    {
                        CurrentGame.GameResult = GameState.Finished;
                        //if (!_checkForWinnerManager.CheckForWinner())
                        //{
                        //    if (_checkForWinnerManager.CheckForTie())
                        //    {
                        //        CurrentGame.Winner = PlayerType.None;
                        //    }
                        //}
                        //else
                        //{
                        //    CurrentGame.Winner = (CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
                        //}
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
            _logger.LogError("Receive opponent Left");
            StateHasChanged?.Invoke();
        }

        // Player connection

        public bool isReceivedConnectedStatus = false;
        public async Task SendConnectedStatus(int roomId, string userId)
        {
            await _gameHubConnection.SendConnectedStatus(roomId, userId, isReceivedConnectedStatus);
            _logger.LogError("I`m connected");
        }
        public void ReceiveConnectedStatus(string userId, bool isAnotherPlayerNotified)
        {
            ReceiveConnectedStatusAsync(userId, isAnotherPlayerNotified).GetAwaiter().GetResult();
        }
        public async Task ReceiveConnectedStatusAsync(string userId, bool isAnotherPlayerNotified)
        {
            if (_gameManager.CurrentPlayer.Id != userId && !_gameManager.IsLoadingNextGame)
            {
                _logger.LogError("Opponent connected");
                if (isReceivedConnectedStatus == false)
                {
                    isReceivedConnectedStatus = true;

                    _waitingAnotherPlayerTimer?.Stop();
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
            _waitingAnotherPlayerTimer?.Stop();

            _moveTimer?.Dispose();
            _responseTimer?.Dispose();
            _waitingAnotherPlayerTimer?.Dispose();
            _logger.LogError("Dispose disconnection timers");
            await _gameHubConnection.DisposeAsync();
        }
    }
}
