using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TicTacToeGame.Services.HubConnections
{
    // TODO:

    // Remove int from SendGameState , SendGameStatus

    // maybe add strategy pattern for sending messages

    public class GameHubConnection : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        private readonly NavigationManager _navigationManager;

        private readonly ILogger<GameHubConnection> _logger;

        public event Action<BoardElements[], PlayerType, int> SendGameStateEvent;
        public event Action<GameState, string, int> SendGameStatusEvent;

        public event Action OpponentLeftEvent;
        public event Action<int, string> OpponentLeavesEvent;
        public event Action<int, string> CheckIfOpponentLeavesEvent;
        public event Action<int, string> OpponentNotLeavesEvent;

        public event Action<string, int> AskAnotherPlayerBoardEvent;
        public event Action<string, BoardElements[]> SendAnotherPlayerBoardEvent;

        public event Action<string, string> ReceiveChatMessageEvent;

        public event Action<string> AskAnotherPlayerForNextGameEvent;
        public event Action<string> DeclineAnotherGameRequestEvent;
        public event Action<string> AcceptAnotherGameRequestEvent;
        public event Action<string> JoinNextGameEvent;


        public GameHubConnection(ILogger<GameHubConnection> logger, NavigationManager navigationManager)
        {
            _logger = logger;
            _navigationManager = navigationManager;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/gamehub"))
                .Build();

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            _hubConnection.On<BoardElements[], PlayerType, int>("SendGameState", (receivedBoard, nextPlayerTurn, gameId)
                => SendGameStateEvent?.Invoke(receivedBoard, nextPlayerTurn, gameId));

            _hubConnection.On<GameState, string, int>("SendGameStatus", (receiveGameResult, receiveGameStatus, gameId)
                => SendGameStatusEvent?.Invoke(receiveGameResult, receiveGameStatus, gameId));

            _hubConnection.On("OpponentLeft", () => OpponentLeftEvent?.Invoke());

            _hubConnection.On<string, int>("AskAnotherPlayerBoard", (userId, gameId) =>
            {
                Task.Run(() => AskAnotherPlayerBoardEvent?.Invoke(userId, gameId)).Wait();
            });
            _hubConnection.On<string, BoardElements[]>("SendAnotherPlayerBoard", (userId, playerBoard)
                => SendAnotherPlayerBoardEvent?.Invoke(userId, playerBoard));

            _hubConnection.On<int, string>("OpponentLeaves", (gameId, connectionId)
                => OpponentLeavesEvent?.Invoke(gameId, connectionId));

            _hubConnection.On<int, string>("CheckIfOpponentLeaves", (roomId, connectionId) =>
            {
                Task.Run(() => CheckIfOpponentLeavesEvent?.Invoke(roomId, connectionId)).Wait();
            });

            _hubConnection.On<int, string>("OpponentNotLeaves", (roomId, connectionId)
                => OpponentNotLeavesEvent?.Invoke(roomId, connectionId));

            _hubConnection.On<string, string>("ReceiveChatMessage", (playerNickname, message)
                => ReceiveChatMessageEvent?.Invoke(playerNickname, message));

            _hubConnection.On<string>("AskAnotherPlayerForNextGame", (userId)
                => AskAnotherPlayerForNextGameEvent?.Invoke(userId));

            _hubConnection.On<string>("DeclineAnotherGameRequest", (userId)
                => DeclineAnotherGameRequestEvent?.Invoke(userId));

            _hubConnection.On<string>("AcceptAnotherGameRequest", (userId) =>
            {
                Task.Run(() => AcceptAnotherGameRequestEvent?.Invoke(userId)).Wait();
            });

            _hubConnection.On<string>("JoinNextGame", (userId)
                => JoinNextGameEvent?.Invoke(userId));
        }

        // Game component
        public async Task SendAnotherPlayerBoard(int currentGameRoomId, string currentUserId, BoardElements[] board)
        {
            await _hubConnection.SendAsync("SendAnotherPlayerBoard", currentGameRoomId, currentUserId, board);
        }
        public async Task JoinGame(int gameRoomId)
        {
            await _hubConnection.SendAsync("JoinGame", gameRoomId);
        }
        public async Task AskAnotherPlayerBoard(int gameId, string userId)
        {
            await _hubConnection.SendAsync("AskAnotherPlayerBoard", gameId, userId);
        }
        // PlayerDisconectingTrackingService
        public async Task CheckIfOpponentLeaves(int roomId, string gameConnectionId)
        {
            await _hubConnection.SendAsync("CheckIfOpponentLeaves", roomId, gameConnectionId);
        }
        public async Task OpponentLeft(int roomId)
        {
            await _hubConnection.SendAsync("OpponentLeft", roomId);
        }
        public async Task OpponentNotLeaves(int roomId, string gameConnectionId)
        {
            await _hubConnection.SendAsync("OpponentNotLeaves", roomId, gameConnectionId);
        }
        // Chat logic
        public async Task SendChatMessage(int roomId, string playerNickname, string message)
        {
            await _hubConnection.SendAsync("SendChatMessage", roomId, playerNickname, message);
        }
        // Next Game logic
        public async Task JoinNextGame(int roomId, string userId)
        {
            await _hubConnection.SendAsync("JoinNextGame", roomId, userId);
        }
        public async Task AcceptAnotherGameRequest(int roomId, string userId)
        {
            await _hubConnection.SendAsync("AcceptAnotherGameRequest", roomId, userId);
        }
        public async Task DeclineAnotherGameRequest(int roomId, string userId)
        {
            await _hubConnection.SendAsync("DeclineAnotherGameRequest", roomId, userId);
        }
        public async Task AskAnotherPlayerForNextGame(int roomId, string userId)
        {
            await _hubConnection.SendAsync("AskAnotherPlayerForNextGame", roomId, userId);
        }
        // MakeMovesGameManager
        public async Task SendGameStatus(GameState gameResult, string gameStatus, int gameId)
        {
            await _hubConnection.SendAsync("SendGameStatus", gameResult, gameStatus, gameId);
        }
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn, int roomId)
        {
            await _hubConnection.SendAsync("SendGameState", board, nextPlayerTurn, roomId);
        }
        public async Task StartConnectionAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting HubConnection");
            }
        }

        public string? GetConnectionId() => _hubConnection.ConnectionId;

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_hubConnection.State != HubConnectionState.Disconnected)
                {
                    await _hubConnection.StopAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing HubConnection");
            }
            finally
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
