using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameChatService
    {
        public List<KeyValuePair<string, string>> ChatMessages = new List<KeyValuePair<string, string>>();
        //private readonly GameHubConnection _gameHubConnection;

        private Game _currentGame;
        private Player _sender;
        private HubConnection _hubConnection;
        public bool NewMessage { get; set; } = false;


        public event Action UpdateComponent;

        public bool _isChatVisible = false;

        //public GameChatService(GameHubConnection gameHubConnection)
        //{
        //    _gameHubConnection = gameHubConnection;
        //}

        public void SetHubConnection(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;

            hubConnection.On<string, string>("ReceiveChatMessage", (playerNickname, message) => AddMessage(playerNickname, message));
        }

        public void SetSender(Game currentGame, Player sender)
        {
            _currentGame = currentGame;
            _sender = sender;
        }

        public void AddMessage(string playerNickname, string message)
        {
            ChatMessages.Add(new KeyValuePair<string, string>(playerNickname, message));

            if (!_isChatVisible)
                NewMessage = true;

            UpdateComponent?.Invoke();
        }
        public async Task SendMessage(string message)
        {
            NewMessage = false;
            await _hubConnection.SendAsync("SendChatMessage", _currentGame.RoomId, _sender.UserName, message);
            //await _gameHubConnection.SendChatMessage((int)_currentGame.RoomId, _sender.UserName, message);
            UpdateComponent?.Invoke();

        }

        public void ToggleChat()
        {
            if (!_isChatVisible)
                NewMessage = false;

            _isChatVisible = !_isChatVisible;

            UpdateComponent?.Invoke();
        }
    }
}
