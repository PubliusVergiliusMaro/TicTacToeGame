using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Services.HubConnections
{
    public class GameHubConnection : IAsyncDisposable
    {
        public readonly HubConnection _hubConnection;

        private readonly NavigationManager _navigationManager;

        // Moves
        public event Action<BoardElements[], PlayerType, int> ReceiveGameStateEvent;
        public event Action<GameState, string, int> ReceiveGameStatusEvent;

        // Disconnections
        public event Action ReceiveOpponentLeftEvent;
        public event Action<int, string> ReceiveOpponentLeavesEvent;
        public event Action<int, string> ReceiveIfOpponentLeavesEvent;
        public event Action<int, string> ReceiveOpponentNotLeavesEvent;
        public event Action<string> ReceiveReloadTimersEvent;

        // Connection
        public event Action<string, bool> ReceiveConnectedStatusEvent;

        // Get board
        public event Action<string, int> AskToReceiveAnotherPlayerBoardEvent;
        public event Action<string, BoardElements[]> ReceiveAnotherPlayerBoardEvent;

        // Chat
        public event Action<string, string> ReceiveChatMessageEvent;

        // Next game
        public event Action<string> ReceiveAnotherPlayerAnswerForNextGameEvent;
        public event Action<string> ReceiveDeclineAnotherGameRequestEvent;
        //public event Action<string> ReceiveAcceptAnotherGameRequestEvent;
        //public event Action<string> ReceiveJoinningToNextGameEvent;

        public event Action ReceiveReadyNextGameStatusEvent;

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

            _hubConnection.On<string>("ReceiveReloadTimers", (userId) => ReceiveReloadTimersEvent?.Invoke(userId));
            // Connection
            _hubConnection.On<string, bool>("ReceiveConnectedStatus", (userId, isAnotherPlayerNotified)
                               => ReceiveConnectedStatusEvent?.Invoke(userId, isAnotherPlayerNotified));
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

            //_hubConnection.On<string>("ReceiveAcceptAnotherGameRequest", (userId) =>
            //{
            //    Task.Run(() => ReceiveAcceptAnotherGameRequestEvent?.Invoke(userId)).Wait();
            //});

            //_hubConnection.On<string>("ReceiveJoinningToNextGame", (userId)
            //    => ReceiveJoinningToNextGameEvent?.Invoke(userId));

            _hubConnection.On("ReceiveReadyNextGameStatus", ()
                => ReceiveReadyNextGameStatusEvent?.Invoke());
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
        public async Task SendOpponentNotLeaves(int roomId, string gameConnectionId)
        {
            await _hubConnection.SendAsync("SendOpponentNotLeaves", roomId, gameConnectionId);
        }
        public async Task SendUserLeftMessageToTheRoom(int roomId)
        {
            await _hubConnection.SendAsync("SendOpponentLeft", roomId);
        }
        public async Task SendUserLeaves(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendUserLeaves", roomId, userId);
        }
        public async Task SendReloadTimers(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendReloadTimers", roomId, userId);
        }
        // Connection logic
        public async Task SendConnectedStatus(int roomId, string userId, bool isAnotherPlayerNotified)
        {
            await _hubConnection.SendAsync("SendConnectedStatus", roomId, userId, isAnotherPlayerNotified);
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
        //public async Task AskToJoinNextGame(int roomId, string userId)
        //{
        //    await _hubConnection.SendAsync("AskToJoinNextGame", roomId, userId);
        //}
        //public async Task SendAcceptAnotherGameRequest(int roomId, string userId)
        //{
        //    await _hubConnection.SendAsync("SendAcceptAnotherGameRequest", roomId, userId);
        //}
        public async Task SendDeclineAnotherGameRequest(int roomId, string userId)
        {
            await _hubConnection.SendAsync("SendDeclineAnotherGameRequest", roomId, userId);
        }
        public async Task SendReadyNextGameStatus(int roomId)
        {
            await _hubConnection.SendAsync("SendReadyNextGameStatus", roomId);
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
