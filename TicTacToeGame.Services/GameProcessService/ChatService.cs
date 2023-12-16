using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameChatService
    {
        public List<KeyValuePair<string, string>> ChatMessages = new List<KeyValuePair<string, string>>();

        private readonly GameHubConnection _gameHubConnection;
        private readonly GameManager _gameManager;

        public bool NewMessage { get; set; } = false;

        public event Action UpdateComponent;

        public bool _isChatVisible = false;

        public GameChatService(GameManager gameManager, GameHubConnection gameHubConnection)
        {
            _gameManager = gameManager;
            _gameHubConnection = gameHubConnection;
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
            await _gameHubConnection.SendChatMessage((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.UserName, message);
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