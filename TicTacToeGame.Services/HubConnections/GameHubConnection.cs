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
    public class GameHubConnection : IAsyncDisposable
    {
        public readonly HubConnection _hubConnection;

        private readonly NavigationManager _navigationManager;

        public event Action<BoardElements[], PlayerType, int> ReceiveGameStateEvent;
        public event Action<GameState, string, int> ReceiveGameStatusEvent;

        public event Action ReceiveOpponentLeftEvent;
        public event Action<int, string> ReceiveOpponentLeavesEvent;
        public event Action<int, string> ReceiveIfOpponentLeavesEvent;
        public event Action<int, string> ReceiveOpponentNotLeavesEvent;

        public event Action<string, int> AskToReceiveAnotherPlayerBoardEvent;
        public event Action<string, BoardElements[]> ReceiveAnotherPlayerBoardEvent;

        public event Action<string, string> ReceiveChatMessageEvent;

        public event Action<string> ReceiveAnotherPlayerAnswerForNextGameEvent;
        public event Action<string> ReceiveDeclineAnotherGameRequestEvent;
        public event Action<string> ReceiveAcceptAnotherGameRequestEvent;
        public event Action<string> ReceiveJoinningToNextGameEvent;


        public GameHubConnection(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigationManager.ToAbsoluteUri("/gamehub"))
                .Build();

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // Moves
            _hubConnection.On<BoardElements[], PlayerType, int>("ReceiveGameState", (receivedBoard, nextPlayerTurn, gameId)
                => ReceiveGameStateEvent?.Invoke(receivedBoard, nextPlayerTurn, gameId));

            _hubConnection.On<GameState, string, int>("ReceiveGameStatus", (receiveGameResult, receiveGameStatus, gameId)
                => ReceiveGameStatusEvent?.Invoke(receiveGameResult, receiveGameStatus, gameId));

            // Disconnections
            _hubConnection.On("ReceiveOpponentLeft", () => ReceiveOpponentLeftEvent?.Invoke());

            _hubConnection.On<int, string>("ReceiveOpponentLeaves", (gameId, connectionId)
                => ReceiveOpponentLeavesEvent?.Invoke(gameId, connectionId));

            _hubConnection.On<int, string>("ReceiveIfOpponentLeaves", (roomId, connectionId) =>
            {
                Task.Run(() => ReceiveIfOpponentLeavesEvent?.Invoke(roomId, connectionId)).Wait();
            });

            _hubConnection.On<int, string>("ReceiveOpponentNotLeaves", (roomId, connectionId)
                => ReceiveOpponentNotLeavesEvent?.Invoke(roomId, connectionId));

            // Get board
            _hubConnection.On<string, int>("AskToReceiveAnotherPlayerBoard", (userId, gameId) =>
            {
                Task.Run(() => AskToReceiveAnotherPlayerBoardEvent?.Invoke(userId, gameId)).Wait();
            });

            _hubConnection.On<string, BoardElements[]>("ReceiveAnotherPlayerBoard", (userId, playerBoard)
                => ReceiveAnotherPlayerBoardEvent?.Invoke(userId, playerBoard));

            // Chat
            _hubConnection.On<string, string>("ReceiveChatMessage", (playerNickname, message)
                => ReceiveChatMessageEvent?.Invoke(playerNickname, message));

            // Next game
            _hubConnection.On<string>("ReceiveAnotherPlayerAnswerForNextGame", (userId)
                => ReceiveAnotherPlayerAnswerForNextGameEvent?.Invoke(userId));

            _hubConnection.On<string>("ReceiveDeclineAnotherGameRequest", (userId)
                => ReceiveDeclineAnotherGameRequestEvent?.Invoke(userId));

            _hubConnection.On<string>("ReceiveAcceptAnotherGameRequest", (userId) =>
            {
                Task.Run(() => ReceiveAcceptAnotherGameRequestEvent?.Invoke(userId)).Wait();
            });

            _hubConnection.On<string>("ReceiveJoinningToNextGame", (userId)
                => ReceiveJoinningToNextGameEvent?.Invoke(userId));
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
        public async Task SendOpponentLeft(int roomId)
        {
            await _hubConnection.SendAsync("SendOpponentLeft", roomId);
        }
        public async Task SendOpponentNotLeaves(int roomId, string gameConnectionId)
        {
            await _hubConnection.SendAsync("SendOpponentNotLeaves", roomId, gameConnectionId);
        }
        public async Task SendUserLeaves(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendUserLeaves", roomId, userId);
        }
        // Chat logic
        public async Task SendChatMessage(int roomId, string playerNickname, string message)
        {
            await _hubConnection.SendAsync("SendChatMessage", roomId, playerNickname, message);
        }
        // Next Game logic
        public async Task AskAnotherPlayerForNextGame(int roomId, string userId)
        {
            await _hubConnection.SendAsync("AskAnotherPlayerForNextGame", roomId, userId);
        }
        public async Task AskToJoinNextGame(int roomId, string userId)
        {
            await _hubConnection.SendAsync("AskToJoinNextGame", roomId, userId);
        }
        public async Task SendAcceptAnotherGameRequest(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendAcceptAnotherGameRequest", roomId, userId);
        }
        public async Task SendDeclineAnotherGameRequest(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendDeclineAnotherGameRequest", roomId, userId);
        }
        // MakeMovesGameManager
        public async Task SendGameStatus(GameState gameResult, string gameStatus, int gameId)
        {
            await _hubConnection.SendAsync("SendGameStatus", gameResult, gameStatus, gameId);
        }
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn, int roomId)
        {
            try
            {
                await _hubConnection.SendAsync("SendGameState", board, nextPlayerTurn, roomId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task StartConnectionAsync()
        {
            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
                throw new Exception(ex.Message);
            }
            finally
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
