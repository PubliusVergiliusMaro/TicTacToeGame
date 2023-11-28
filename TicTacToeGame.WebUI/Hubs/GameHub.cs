using Microsoft.AspNetCore.SignalR;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinRoom(int roomId, Player player)
        {
            await Clients.All.SendAsync("JoinRoom", roomId, player);
        }
    }
}
