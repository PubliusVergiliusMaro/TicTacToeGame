using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Hubs
{
    public class GameHub : Hub
    {
        public const string HubUrl = "/gamehub";
        //HostRoom
        public async Task JoinRoom(int roomId, Player player)
        {
            await Clients.All.SendAsync("JoinRoom", roomId, player);
        }
        //JoinRoom
        public async Task AcceptJoining(int roomConnectionId, int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.All.SendAsync("AcceptJoining", roomConnectionId, roomId);
        }
        public async Task DeclineJoining(string message,string userId)
        {
            await Clients.All.SendAsync("DeclineJoining", message, userId);
        }
        //Game
        public async Task JoinGame(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }
        // Moves
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn, int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveGameState", board, nextPlayerTurn, roomId);
        }
        public async Task SendGameStatus(GameState gameState, string gameStatus, PlayerType winner, Game game, int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveGameStatus", gameState, gameStatus, winner, game, roomId);
        }
        // Connection
        public async Task SendConnectedStatus(int roomId, string userId,bool isAnotherPlayerNotified)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveConnectedStatus", userId, isAnotherPlayerNotified);
        }
        // Disconnections
        public async Task CheckIfOpponentLeaves(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveIfOpponentLeaves", roomId, userId);
        }
        public async Task SendOpponentNotLeaves(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveOpponentNotLeaves", roomId, userId);
        }
        public async Task SendUserLeaves(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveOpponentLeaves", roomId, userId);
        }
        public async Task SendOpponentLeft(int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveOpponentLeft");
        }
        public async Task SendReloadTimers(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveReloadTimers", userId);
        }
        // Get board
        public async Task AskAnotherPlayerBoard(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("AskToReceiveAnotherPlayerBoard", userId, roomId);
        }
        public async Task SendAnotherPlayerBoard(int roomId, string userId, BoardElements[] board)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveAnotherPlayerBoard", userId, board);
        }
        // Chat
        public async Task SendChatMessage(int roomId, string senderNickname, string message)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", senderNickname, message);
        }
        // Next game

        public async Task AskAnotherPlayerForNextGame(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveAnotherPlayerAnswerForNextGame", userId);
        }
        public async Task SendDeclineAnotherGameRequest(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveDeclineAnotherGameRequest", userId);
        }
        //public async Task SendAcceptAnotherGameRequest(int roomId, string userId)
        //{
        //    await Clients.Group(roomId.ToString()).SendAsync("ReceiveAcceptAnotherGameRequest", userId);
        //}
        //public async Task AskToJoinNextGame(int roomId, string userId)
        //{
        //    await Clients.Group(roomId.ToString()).SendAsync("ReceiveJoinningToNextGame", userId);
        //}
        public async Task SendReadyNextGameStatus(int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveReadyNextGameStatus");
        }
    }
}
