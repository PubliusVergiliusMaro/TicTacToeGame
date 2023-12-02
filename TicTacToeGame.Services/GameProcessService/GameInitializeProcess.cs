﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;
public class GameInitializeProcess :GameManagerBase
{
    public Game CurrentGame { get; set; } = new();
    private readonly GameRepository _gameRepository;
    private readonly NavigationManager _navigationManager;

    public GameInitializeProcess(AuthenticationStateProvider authenticationStateProvider, GameRepository gameRepository, NavigationManager navigationManager)
        : base(authenticationStateProvider)
    {
        _gameRepository = gameRepository;
        _navigationManager = navigationManager;
    }
    public async Task<ClaimsPrincipal> HandleGameForAuthenticatedUser(AuthenticationState authState)
    {
        authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (IsAuthenticatedUserWithGame(user, out var userId))
        {
            CurrentGame = _gameRepository.GetByUsersId(userId);
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
            return _gameRepository.GetByUsersId(userId) is not null;
        }

        return false;
    }
   
}