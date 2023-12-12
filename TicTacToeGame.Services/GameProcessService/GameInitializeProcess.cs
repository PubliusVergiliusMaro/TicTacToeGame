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

    public GameInitializeProcess(AuthenticationStateProvider authenticationStateProvider, GameRepository gameRepository, NavigationManager navigationManager, GameReconnectingService gameReconnectingService)
        : base(authenticationStateProvider)
    {
        _gameRepository = gameRepository;
        _navigationManager = navigationManager;
        _gameReconnectingService = gameReconnectingService;
    }

    public async Task<ClaimsPrincipal> HandleGameForAuthenticatedUser(AuthenticationState authState)
    {
        authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;


        if (IsAuthenticatedUserWithGame(user, out var userId)&&CurrentGame.GameResult==GameState.Starting)
        {
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
