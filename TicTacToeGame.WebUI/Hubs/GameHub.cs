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
        public async Task AcceptJoining(int roomId, Guid gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
            await Clients.All.SendAsync("AcceptJoining", roomId,gameId);
        }
        public async Task DeclineJoining(string message)
        {
            await Clients.All.SendAsync("DeclineJoining", message);
        }
        //Game
        public async Task JoinGame(Guid gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn,Guid gameId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("SendGameState", board, nextPlayerTurn, gameId);    
        }
        public async Task SendGameStatus(GameState gameState,string gameStatus, Guid gameId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("SendGameStatus", gameState, gameStatus,gameId);
        }
        public async Task CheckIfOpponentLeaves(Guid gameId, string connectionId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("CheckIfOpponentLeaves", gameId, connectionId);
        }
        public async Task OpponentNotLeaves(Guid gameId, string connectionId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("OpponentNotLeaves", gameId, connectionId);
        }
        public async Task UserLeaves(Guid gameId, string connectionId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("OpponentLeaves", gameId, connectionId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendChatMessage(Guid gameId, string senderNickname,string message)
        {
            await Clients.Group(gameId.ToString()).SendAsync("ReceiveChatMessage", senderNickname, message);
        }
    }
}
