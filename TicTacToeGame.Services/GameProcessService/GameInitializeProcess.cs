using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;
public class GameInitializeProcess : GameManagerBase
{
    public Game CurrentGame { get; set; } = new();
    private readonly GameRepository _gameRepository;
    private readonly NavigationManager _navigationManager;
    private readonly GameReconnectingService _gameReconnectingService;
    private readonly PlayerRepository _playerRepository;
    public bool IsPlayerAlreadyPlaying { get; set; } = false;
    public GameInitializeProcess(AuthenticationStateProvider authenticationStateProvider, 
        GameRepository gameRepository, NavigationManager navigationManager, 
        GameReconnectingService gameReconnectingService, PlayerRepository playerRepository)
        : base(authenticationStateProvider)
    {
        _gameRepository = gameRepository;
        _navigationManager = navigationManager;
        _gameReconnectingService = gameReconnectingService;
        _playerRepository = playerRepository;
    }

    public async Task<ClaimsPrincipal> HandleGameForAuthenticatedUser(AuthenticationState authState)
    {
        authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;


        if (IsAuthenticatedUserWithGame(user, out var userId)&&CurrentGame.GameResult==GameState.Starting)
        {
            Player currentPlayer = _playerRepository.GetById(userId);
            if (currentPlayer.IsPlaying == true)
            {
                IsPlayerAlreadyPlaying = true;
            }

            _gameReconnectingService.CheckIfPlayerIsAlreadyPlaying(userId);

            return user;
        }
        else
        {
            _navigationManager.NavigateTo("/");
            return null;
        }

    }

    private bool IsAuthenticatedUserWithGame(ClaimsPrincipal? user, out string userId)
    {
        userId = string.Empty;

        if (user?.Identity?.IsAuthenticated == true)
        {
            userId = user.Claims.First().Value.ToString();

            CurrentGame = _gameRepository.GetByUsersId(userId);
            return CurrentGame is not null;
        }

        return false;
    }

}
