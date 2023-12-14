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
            await Clients.All.SendAsync("AcceptJoining", roomConnectionId,roomId);
        }
        public async Task DeclineJoining(string message)
        {
            await Clients.All.SendAsync("DeclineJoining", message);
        }
        //Game
        public async Task JoinGame(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }
        // moves
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn, int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("SendGameState", board, nextPlayerTurn, roomId);    
        }
        public async Task SendGameStatus(GameState gameState,string gameStatus, int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("SendGameStatus", gameState, gameStatus, roomId);
        }
        // disconections
        public async Task CheckIfOpponentLeaves(int roomId, string connectionId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("CheckIfOpponentLeaves", roomId, connectionId);
        }
        public async Task OpponentNotLeaves(int roomId, string connectionId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("OpponentNotLeaves", roomId, connectionId);
        }
        public async Task UserLeaves(int roomId, string connectionId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("OpponentLeaves", roomId, connectionId);
        }
        public async Task OpponentLeft(int roomId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("OpponentLeft");
        }
        // get board
        public async Task AskAnotherPlayerBoard(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("AskAnotherPlayerBoard", userId, roomId);
        }
        public async Task SendAnotherPlayerBoard(int roomId, string userId, BoardElements[] board)
        {
            await Clients.Group(roomId.ToString()).SendAsync("SendAnotherPlayerBoard",userId,board);
        }
        // chat
        public async Task SendChatMessage(int roomId, string senderNickname,string message)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveChatMessage", senderNickname, message);
        }
        // next game

        public async Task AskAnotherPlayerForNextGame(int roomId,string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("AskAnotherPlayerForNextGame", userId);
        }
        public async Task DeclineAnotherGameRequest(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("DeclineAnotherGameRequest", userId);
        }
        public async Task AcceptAnotherGameRequest(int roomId, string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("AcceptAnotherGameRequest", userId);
        }
        public async Task JoinNextGame(int roomId,string userId)
        {
            await Clients.Group(roomId.ToString()).SendAsync("JoinNextGame", userId);
        }   
    }
}
