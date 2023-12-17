﻿using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameChatService
    {
        public List<KeyValuePair<string, string>> ChatMessages = new List<KeyValuePair<string, string>>();

        public string Message { get; set; }

        private readonly GameHubConnection _gameHubConnection;

        private readonly GameManager _gameManager;

        public bool IsReceivedNewMessage { get; set; } = false;

        public bool IsChatVisible = false;

        public event Action StateHasChanged;

        public GameChatService(GameManager gameManager, GameHubConnection gameHubConnection)
        {
            _gameManager = gameManager;
            _gameHubConnection = gameHubConnection;
        }

        public void AddMessage(string playerNickname, string message)
        {
            ChatMessages.Add(new KeyValuePair<string, string>(playerNickname, message));

            if (!IsChatVisible)
                IsReceivedNewMessage = true;

            StateHasChanged?.Invoke();
        }

        public async Task SendMessage(string message)
        {
            IsReceivedNewMessage = false;
            await _gameHubConnection.SendChatMessage((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.UserName, message);
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