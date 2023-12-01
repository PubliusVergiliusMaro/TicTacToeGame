using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Hubs
{
    public class GameHub : Hub
    {
        public async Task AcceptJoining(int roomId, Guid gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
            await Clients.All.SendAsync("AcceptJoining", roomId,gameId);
        }
        public async Task JoinRoom(int roomId, Player player)
        {
            await Clients.All.SendAsync("JoinRoom", roomId, player);
        }
        
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn,Guid gameId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("SendGameState", board, nextPlayerTurn, gameId);
            //await Clients.All.SendAsync("SendGameState", board, nextPlayerTurn,gameId);
        }
        public async Task SendGameStatus(GameState gameState,string gameStatus, Guid gameId)
        {
            await Clients.Group(gameId.ToString()).SendAsync("SendGameStatus", gameState, gameStatus,gameId);
            //await Clients.All.SendAsync("SendGameStatus", gameState, gameStatus,gameId);
        }
        public async Task JoinGame(Guid gameId)
        {
             await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        }
        public async Task DeclineJoining(string message)
        {
            await Clients.All.SendAsync("DeclineJoining", message);
        }
        public async Task SendGroupMessage(Guid gameId)
        {
            //await Clients.All.SendAsync("SendGroupMessage", gameId.ToString());
            await Clients.Group(gameId.ToString()).SendAsync("SendGroupMessage",gameId.ToString());
        }
        //public async Task ReceiveGroupMessage(Guid gameId)
        //{
        //    await Clients.Group(gameId.ToString()).SendAsync("ReceiveGroupMessage",gameId.ToString());
        //}
        public async Task LeaveGame(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        public async Task SendMessageToGroup(Guid gameId, string message)
        {
            await Clients.Group(gameId.ToString()).SendAsync("ReceiveMessage", message);
        }

        public override async Task OnConnectedAsync()// Implement
        {
            Console.WriteLine(Context.ConnectionId + " connected.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
