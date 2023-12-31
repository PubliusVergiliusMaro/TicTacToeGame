﻿using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class ChatMessage
    {
        public string PlayerId { get; set; }
        public string PlayerNickname { get; set; }
        public string Message { get; set; }
        public int RoomId { get; set; }
        public DateTime SendTime { get; set; } = DateTime.UtcNow;
    }

    public class GameChatService
    {
        public static List<ChatMessage> ChatMessages = new List<ChatMessage>();

        public string Message { get; set; }

        private GameHubConnection _gameHubConnection;

        private readonly GameManager _gameManager;

        public bool IsReceivedNewMessage { get; set; } = false;

        public bool IsChatVisible = false;

        public event Action StateHasChanged;

        public void SetHubConnection(GameHubConnection gameHubConnection)
        {
            _gameHubConnection = gameHubConnection;
        }
        public GameChatService(GameManager gameManager, GameHubConnection gameHubConnection)
        {
            _gameManager = gameManager;
            _gameHubConnection = gameHubConnection;
        }

        public void ReceiveChatMessage(string playerNickname, string message)
        {
            if (!IsChatVisible)
                IsReceivedNewMessage = true;

            StateHasChanged?.Invoke();
        }

        public async Task SendMessage(string message, Player player, int CurrentRoom)
        {
            if(string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var newMessage = new ChatMessage
            {
                PlayerId = player.Id,
                PlayerNickname = player.UserName,
                Message = message,
                RoomId = CurrentRoom,
                SendTime = DateTime.UtcNow,
            };
            // Add the new message to the list
            ChatMessages.Add(newMessage);

            // Send the chat message through SignalR
            await _gameHubConnection.SendChatMessage((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.UserName, message);

            // Update the component
            StateHasChanged?.Invoke();
        }
        public void ToggleChat()
        {
            if (!IsChatVisible)
                IsReceivedNewMessage = false;

            IsChatVisible = !IsChatVisible;

            StateHasChanged?.Invoke();
        }
    }
}