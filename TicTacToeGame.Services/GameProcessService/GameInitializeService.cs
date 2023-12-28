using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;
public class GameInitializeService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    private readonly GameRepository _gameRepository;
    private readonly PlayerRepository _playerRepository;

    private readonly GameReconnectingService _gameReconnectingService;

    private readonly GameBoardManager _gameBoardManager;

    private readonly GameManager _gameManager;

    private readonly ILogger<GameInitializeService> _logger;
    
    public GameInitializeService(AuthenticationStateProvider authenticationStateProvider,
        GameReconnectingService gameReconnectingService,
        GameRepository gameRepository,
        PlayerRepository playerRepository,
        GameManager gameManager,
        GameBoardManager gameBoardManager,
        ILogger<GameInitializeService> logger)
    {
        _gameRepository = gameRepository;
        _gameReconnectingService = gameReconnectingService;
        _playerRepository = playerRepository;
        _authenticationStateProvider = authenticationStateProvider;
        _gameManager = gameManager;
        _gameBoardManager = gameBoardManager;
        _logger = logger;
    }

    public async Task<bool> InitializeGameForAuthenticatedUser()
    {
        await _gameManager.InitializeAuthState(_authenticationStateProvider);

        bool isUserAuthorized = _gameManager.IsAuthenticatedUser();
        _logger.LogError($"Is Player Authorized");
        if (isUserAuthorized)
        {
            bool isUserAlreadyPlaying = _gameReconnectingService.CheckIfPlayerIsAlreadyPlaying(_gameManager.GetCurrentUserId());
            _logger.LogError($"Is Player Already Playing - {isUserAlreadyPlaying}");
            if (!isUserAlreadyPlaying)
            {
                _gameManager.ClearData();

                _logger.LogError($"Is gameManager initialized before {_gameManager.IsInitialized}");
                
                _gameManager.IsInitialized = true;

                _logger.LogError($"Is gameManager initialized after {_gameManager.IsInitialized}");

                await _gameManager.InitializeAuthState(_authenticationStateProvider);
                _logger.LogError($"Initialize AuthState");
                _gameManager.InitializeRepositories(_gameRepository, _playerRepository);
                _logger.LogError($"Initialize Repositories");
                bool isSuccesfullyGame = _gameManager.InitializeGame();
                _logger.LogError($"Initialize Game");
                bool isSuccesfullyPlayer = _gameManager.InitializePlayers();
                _logger.LogError($"Initialize Players");
                if (!isSuccesfullyGame) 
                {
                    _logger.LogError($"not isSuccesfullyGame");
                    return false;
                }
                if(!isSuccesfullyPlayer)
                {
                    _logger.LogError($"not isSuccessfullyPlayer");
                }
                    AddBoardToManager();
                
                return true;
            }
            else
            {
                _logger.LogError($"is already playing");
                return false;
            }
        }
        else
        {
            _logger.LogError($"not authorized");
            
            return false;
        }
    }
    private void AddBoardToManager()
    {
        _gameBoardManager.AddBoardIfNotExist(_gameManager.CurrentGame.UniqueId, _gameManager.Board);

        _gameManager.Board = _gameBoardManager.GetBoard(_gameManager.CurrentGame.UniqueId);
    }
}
