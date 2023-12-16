using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;
public class GameInitializeService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    
    private readonly GameRepository _gameRepository;
    private readonly PlayerRepository _playerRepository;
    
    private readonly GameReconnectingService _gameReconnectingService;
    
    private readonly GameManager _gameManager;
    public bool IsPlayerAlreadyPlaying { get; set; } = false;
    
    public GameInitializeService(AuthenticationStateProvider authenticationStateProvider,
        GameReconnectingService gameReconnectingService, 
        GameRepository gameRepository, 
        PlayerRepository playerRepository,
        GameManager gameManager)
    {
        _gameRepository = gameRepository;
        _gameReconnectingService = gameReconnectingService;
        _playerRepository = playerRepository;
        _authenticationStateProvider = authenticationStateProvider;
        _gameManager = gameManager;
    }

    public async Task<bool> InitializeGameForAuthenticatedUser()
    {
        await _gameManager.InitializeAuthState(_authenticationStateProvider);

        bool isUserAuthorized = IsAuthenticatedUser();

        if (isUserAuthorized)
        {
            bool isUserAlreadyPlaying = _gameReconnectingService.CheckIfPlayerIsAlreadyPlaying(_gameManager.GetCurrentUserId());

            if (!isUserAlreadyPlaying)
            {
                _gameManager.InitializeRepositories(_gameRepository,_playerRepository);
                _gameManager.InitializeGame();
                _gameManager.InitializePlayers();

                return true;
            }
            else
            {
                IsPlayerAlreadyPlaying = true;
                return false;
            }
        }
        else
        {
            IsPlayerAlreadyPlaying = true;
            return false;
        }
    }
    private bool IsAuthenticatedUser() => _gameManager.ClaimsPrincipal?.Identity?.IsAuthenticated == true;
}
