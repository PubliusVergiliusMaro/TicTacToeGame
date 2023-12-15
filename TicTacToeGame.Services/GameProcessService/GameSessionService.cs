using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameSessionService
    {
        //private HubConnection _hubConnection;
        private readonly GameHubConnection _gameHubConnection;

        private readonly GameRepository _gameRepository;
        private readonly PlayerRepository _playerRepository;
        private readonly NavigationManager _navigationManager;

        public event Action UpdateComponent;

        public bool RequestForNextGame { get; set; } = false;
        public bool IsTwoPlayersPlaying { get; set; } = false;
        public bool ApprovedNextGame { get; set; } = false;
        public bool DeclinedNextGame { get; set; } = false;

        private Player CurrentPlayer;
        private Game CurrentGame;

        public GameSessionService(GameRepository gameRepository,
            PlayerRepository playerRepository,
            NavigationManager navigationManager,
            GameHubConnection gameHubConnection)
        {
            _gameRepository = gameRepository;
            _playerRepository = playerRepository;
            _navigationManager = navigationManager;
            _gameHubConnection = gameHubConnection;
        }

        //public void SetHubConnection(HubConnection hubConnection)
        //{
        //    _hubConnection = hubConnection;

        //    //_hubConnection.On<string>("AskAnotherPlayerForNextGame", (userId) => AskAnotherPlayerForNextGame(userId));

        //    //_hubConnection.On<string>("DeclineAnotherGameRequest", (userId) => DeclineAnotherGameRequest(userId));

        //    //_hubConnection.On<string>("AcceptAnotherGameRequest", (userId) => AcceptAnotherGameRequest(userId));

        //    _hubConnection.On<string>("JoinNextGame", (userId) => JoinNextGame(userId));

        //}

        public void SetCurrentPlayerAndGame(Player player, Game game)
        {
            CurrentPlayer = player;
            CurrentGame = game;
        }
        public void AskAnotherPlayerForNextGame(string userId)
        {
            if (userId != CurrentPlayer.Id)
            {
                RequestForNextGame = true;
                UpdateComponent?.Invoke();
            }
        }
        public void DeclineAnotherGameRequest(string userId)
        {
            ApprovedNextGame = false;
            DeclinedNextGame = true;
            RequestForNextGame = true;
            UpdateComponent?.Invoke();
        }
        public void AcceptAnotherGameRequest(string userId)
        {
            AcceptAnotherGameRequestAsync(userId).GetAwaiter().GetResult();
        }
        public async Task AcceptAnotherGameRequestAsync(string userId)
        {
            if (userId != CurrentPlayer.Id)
            {
                ApprovedNextGame = true;
                DeclinedNextGame = false;
                Random random = new Random();
                Game newGame = new()
                {
                    PlayerHostId = CurrentGame.PlayerHostId,
                    PlayerGuestId = CurrentGame.PlayerGuestId,
                    RoomId = CurrentGame.RoomId,
                    GameResult = GameState.Starting,
                    CurrentTurn = random.Next(3) == 0 ? PlayerType.Host : PlayerType.Guest
                };

                _gameRepository.AddEntity(newGame);
                // make that host becomes such player that sends request
                _playerRepository.UpdatePlayerStatus(CurrentPlayer.Id, true);
                // make new game with same players

                await _gameHubConnection.JoinNextGame((int)CurrentGame.RoomId, CurrentPlayer.Id);
                //await _hubConnection.SendAsync("JoinNextGame", CurrentGame.RoomId, CurrentPlayer.Id);

                _navigationManager.NavigateTo("/game", forceLoad: true);
            }
        }
        public void JoinNextGame(string userId)
        {
            if (userId != CurrentPlayer.Id)
            {
                _playerRepository.UpdatePlayerStatus(CurrentPlayer.Id, true);

                _navigationManager.NavigateTo("/game", forceLoad: true);
            }
        }

        public async Task ApproveNextGamePlayerRequest()
        {
            await _gameHubConnection.AcceptAnotherGameRequest((int)CurrentGame.RoomId, CurrentPlayer.Id);
            //await _hubConnection.SendAsync("AcceptAnotherGameRequest", CurrentGame.RoomId, CurrentPlayer.Id);
        }
        public async Task DeclineNextPlayerRequest()
        {
            await _gameHubConnection.DeclineAnotherGameRequest((int)CurrentGame.RoomId, CurrentPlayer.Id);
            //await _hubConnection.SendAsync("DeclineAnotherGameRequest", CurrentGame.RoomId, CurrentPlayer.Id);

            DeclinedNextGame = true;
            UpdateComponent?.Invoke();
        }
        public async Task PlayNextGame()
        {
            await _gameHubConnection.AskAnotherPlayerForNextGame((int)CurrentGame.RoomId, CurrentPlayer.Id);
            //await _hubConnection.SendAsync("AskAnotherPlayerForNextGame", CurrentGame.RoomId, CurrentPlayer.Id);
        }
    }
}
