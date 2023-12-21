﻿using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameReconnectingService
    {
        private readonly PlayerRepository _playerRepository;
        private readonly GameRepository _gameRepository;

        public bool IsPlayerLeaveGameByButton = false;

        public GameReconnectingService(PlayerRepository playerRepository,
            GameRepository gameRepository)
        {
            _playerRepository=playerRepository;
            _gameRepository=gameRepository;
        }
        public bool CheckIfRecconectPlayer(string userId)
        {
            Player player = _playerRepository.GetById(userId);
            Game game = _gameRepository.GetByUsersId(userId);

            if (game != null && player.IsPlaying == false)
            {
                return true;
            }

            return false;
        }
        public bool CheckIfPlayerIsAlreadyPlaying(string playerId)
        {
            Player CurrentPlayer = _playerRepository.GetById(playerId);

            if (CurrentPlayer.IsPlaying == true)
            {
                return true;
            }
            else
            {
                MakePlayerPlaying(playerId);
                return false;
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
