using Microsoft.AspNetCore.Components;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameReconnectingService
    {
        private readonly NavigationManager _navigationManager;
        private readonly PlayerRepository _playerRepository;
        private readonly GameRepository _gameRepository;
        public GameReconnectingService(NavigationManager navigationManager,
            PlayerRepository playerRepository,
            GameRepository gameRepository)
        {
            _navigationManager = navigationManager;
            _playerRepository=playerRepository;
            _gameRepository=gameRepository;
        }
        public void RecconectPlayer(string userId)
        {
            Player player = _playerRepository.GetById(userId);
            Game game = _gameRepository.GetByUsersId(userId);

            if (game != null && player.IsPlaying == false)
            {
                _navigationManager.NavigateTo("/game");
            }
        }
        public void CheckIfPlayerIsAlreadyPlaying(string playerId)
        {
            Player CurrentPlayer = _playerRepository.GetById(playerId);

            if (CurrentPlayer.IsPlaying == true)
            {
                _navigationManager.NavigateTo("/");
            }
            else
            {
                MakePlayerPlaying(playerId);
            }
        }
        public void MakePlayerNotPlaying(string playerId)
        {
            _playerRepository.UpdatePlayerStatus(playerId, false);
        }
        public void MakePlayerPlaying(string playerId)
        {
            _playerRepository.UpdatePlayerStatus(playerId, true);
        }
    }
}
