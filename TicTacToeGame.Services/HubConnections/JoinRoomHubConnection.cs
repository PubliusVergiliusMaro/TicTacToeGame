﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.HubConnections
{
    public class JoinRoomHubConnection : IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        private readonly NavigationManager _navigationManager;

        private readonly ILogger<JoinRoomHubConnection> _logger;

        public event Func<int, int, Task> AcceptJoiningEvent;
        public event Action<string, string> DeclineJoiningEvent;
        public event Action<string, double> ReceiveLatencyEvent;

        public JoinRoomHubConnection(ILogger<JoinRoomHubConnection> logger, NavigationManager navigationManager)
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
            _hubConnection.On<int, int>("AcceptJoining", async (joinedRoomId, gameRoomId) =>
                AcceptJoiningEvent?.Invoke(joinedRoomId, gameRoomId));

            _hubConnection.On<string,string>("DeclineJoining", (declineMessage, playerId) =>
                DeclineJoiningEvent?.Invoke(declineMessage, playerId));

            _hubConnection.On<string,double>("ReceiveLatency", (userId,latency) =>
                ReceiveLatencyEvent?.Invoke(userId,latency));
        }
        public async Task MeasureLatency(string userId)
        {
            await _hubConnection.SendAsync("MeasureLatency", userId);
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

        public async Task JoinRoomAsync(int roomId, Player currentPlayer)
        {
            try
            {
                await _hubConnection.SendAsync("JoinRoom", roomId, currentPlayer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending JoinRoom message");
            }
        }

        public async Task JoinGameAsync(int gameRoomId)
        {
            try
            {
                await _hubConnection.SendAsync("JoinGame", gameRoomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending JoinRoom message");
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
                _logger.LogError(ex, "Error stopping HubConnection");
            }
            finally
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
