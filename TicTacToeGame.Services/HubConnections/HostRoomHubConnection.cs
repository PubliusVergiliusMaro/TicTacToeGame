using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Formats.Asn1;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.RoomServices;

namespace TicTacToeGame.Services.HubConnections
{
    public class HostRoomHubConnection : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        private readonly NavigationManager _navigationManager;
    
        private readonly ILogger<HostRoomHubConnection> _logger;

        public event Action<int,Player> JoinRoomEvent;

        public HostRoomHubConnection(ILogger<HostRoomHubConnection> logger, NavigationManager navigationManager)
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
            _hubConnection.On<int, Player>("JoinRoom", (joinedRoomId, joinPlayer)
                =>
            {
                Task.Run(() => JoinRoomEvent?.Invoke(joinedRoomId, joinPlayer));
            });   
        }

        public async Task JoinGame(int createdRoomId)
        {
            await _hubConnection.SendAsync("JoinGame", createdRoomId);
        }
        public async Task AcceptJoining(int connectionId, int createdRoomId)
        {
            await _hubConnection.SendAsync("AcceptJoining", connectionId, createdRoomId);
        }
        public async Task DeclineJoining(string declineMessage, string playerId)
        {
            await _hubConnection.SendAsync("DeclineJoining", declineMessage, playerId);
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
