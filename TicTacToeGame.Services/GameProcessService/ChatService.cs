using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameChatService
    {
        private readonly GameHubConnection _gameHubConnection;
        public List<KeyValuePair<string, string>> ChatMessages = new List<KeyValuePair<string, string>>();

        private Game _currentGame;
        private Player _sender;

        //private HubConnection _hubConnection;

        public event Action UpdateComponent;

        public bool _isChatVisible = false;
        //public void SetHubConnection(HubConnection hubConnection)
        //{
        //    _hubConnection = hubConnection;

        //    //hubConnection.On<string, string>("ReceiveChatMessage", (playerNickname, message) => 
        //    //AddMessage(playerNickname, message));
        //}
        public GameChatService(GameHubConnection gameHubConnection)
        {
            _gameHubConnection = gameHubConnection;
        }
        public void SetSender(Game currentGame, Player sender)
        {
            _currentGame = currentGame;
            _sender = sender;
        }

        public void AddMessage(string playerNickname, string message)
        {
            ChatMessages.Add(new KeyValuePair<string, string>(playerNickname, message));
            UpdateComponent?.Invoke();
        }
        public async Task SendMessage(string message)
        {
            await _gameHubConnection.SendChatMessage((int)_currentGame.RoomId, _sender.UserName, message);
            //await _hubConnection.SendAsync("SendChatMessage", _currentGame.RoomId, _sender.UserName, message);
        }

        public void ToggleChat()
        {
            _isChatVisible = !_isChatVisible;
            UpdateComponent?.Invoke();
        }
    }
}
