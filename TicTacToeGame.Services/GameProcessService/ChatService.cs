using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameChatService
    {
        public List<KeyValuePair<string, string>> ChatMessages = new List<KeyValuePair<string, string>>();

        private Game _currentGame;
        private Player _sender;
        
        private HubConnection _hubConnection;

        public event Action UpdateComponent;

        public bool _isChatVisible = false;
        public void SetHubConnection(HubConnection hubConnection)
        {
            _hubConnection = hubConnection;

            hubConnection.On<string,string>("ReceiveChatMessage", (playerNickname, message) => AddMessage(playerNickname, message));
        }

        public void SetSender(Game currentGame,  Player sender)
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
            await _hubConnection.SendAsync("SendChatMessage",_currentGame.UniqueId, _sender.UserName ,message);
        }

        public void ToggleChat()
        {
            _isChatVisible = !_isChatVisible;
            UpdateComponent?.Invoke();
        }
    }
}
