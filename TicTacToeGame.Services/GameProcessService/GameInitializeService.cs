using Microsoft.AspNetCore.Components.Authorization;
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

        bool isUserAuthorized = _gameManager.IsAuthenticatedUser();

        if (isUserAuthorized)
        {
            bool isUserAlreadyPlaying = _gameReconnectingService.CheckIfPlayerIsAlreadyPlaying(_gameManager.GetCurrentUserId());

            if (!isUserAlreadyPlaying)
            {
                _gameManager.ClearData();
                await _gameManager.InitializeAuthState(_authenticationStateProvider);
                _gameManager.InitializeRepositories(_gameRepository, _playerRepository);
                bool isSuccesfullyGame = _gameManager.InitializeGame();
                bool isSuccesfullyPlayer = _gameManager.InitializePlayers();
                
                if(!isSuccesfullyGame || !isSuccesfullyPlayer)
                {
                    return false;
                }
                
                _gameManager.IsInitialized = true;
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
}
