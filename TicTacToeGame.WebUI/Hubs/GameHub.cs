using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Hubs
{
    public class GameHub : Hub
    {
        public async Task AcceptJoining(int roomId)
        {
            await Clients.All.SendAsync("AcceptJoining", roomId);
        }
        public async Task JoinRoom(int roomId, Player player)
        {
            await Clients.All.SendAsync("JoinRoom", roomId, player);
        }
        /*public async Task UpdateBoard(int roomId, BoardElements[] updatedBoard)
        {
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveUpdatedBoard", updatedBoard);
        }*/
        public async Task SendGameState(BoardElements[] board, PlayerType nextPlayerTurn)
        {
            await Clients.All.SendAsync("SendGameState", board, nextPlayerTurn);
        }
        public async Task SendGameStatus(GameState gameState,string gameStatus)
        {
            await Clients.All.SendAsync("SendGameStatus", gameState, gameStatus);
        }
        //public async Task JoinGroup(string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //}

        //public async Task SendGameStateToGroup(string groupName, BoardElements[] board, PlayerType nextPlayerTurn)
        //{
        //    await Clients.Group(groupName).SendAsync("SendGameStateToGroup", board, nextPlayerTurn);
        //}

        //public async Task SendGameStatusToGroup(string groupName, GameState gameState, string gameStatus)
        //{
        //    await Clients.Group(groupName).SendAsync("SendGameStatusToGroup", gameState, gameStatus);
        //}
    }
}
