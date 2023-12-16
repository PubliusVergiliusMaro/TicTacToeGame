using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;

// then check if from di will get this game manager base
public class GameManager
{
    public Player CurrentPlayerHost;
    public Player CurrentPlayerGuest;
    public Player CurrentPlayer;

    public Game CurrentGame = new();

    public AuthenticationState AuthenticationState;
    public AuthenticationStateProvider _authenticationStateProvider;
    public ClaimsPrincipal? ClaimsPrincipal = new();

    public GameRepository GameRepository;
    public PlayerRepository PlayerRepository;

    public BoardElements[] Board = new BoardElements[TicTacToeRules.BOARD_SIZE];

    public bool IsInitialized = false;
   
    public async Task InitializeAuthState(AuthenticationStateProvider authenticationStateProvider)
    {
        AuthenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal = AuthenticationState.User;
        IsInitialized = true;
    }
    public void InitializeRepositories(GameRepository gameRepository, PlayerRepository playerRepository)
    {
        if(gameRepository == null || playerRepository == null)
        {
           throw new ArgumentNullException("GameRepository or PlayerRepository is null");
        }
        
        GameRepository = gameRepository;
        PlayerRepository = playerRepository;
    }
    public string GetCurrentUserId()
    {
        if(ClaimsPrincipal == null)
        {
            throw new ArgumentNullException("ClaimsPrincipal is null");
        }

        return ClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
    public void InitializeGame()
    {
        CurrentGame = GameRepository.GetByUsersId(GetCurrentUserId());
        
        if(CurrentGame == null)
        {
            throw new ArgumentNullException("CurrentGame is null");
        }
    }
    public void InitializePlayers()
    {
        if(CurrentGame == null)
        {
           throw new ArgumentNullException("CurrentGame is null");
        }

        CurrentPlayerHost = PlayerRepository.GetById(CurrentGame.PlayerHostId);
        CurrentPlayerGuest = PlayerRepository.GetById(CurrentGame.PlayerGuestId);
        CurrentPlayer = PlayerRepository.GetById(GetCurrentUserId());

        if(CurrentPlayerHost == null || CurrentPlayerGuest == null || CurrentPlayer == null)
        {
           throw new ArgumentNullException("CurrentPlayerHost or CurrentPlayerGuest or CurrentPlayer is null");
        }
    }
    public string GetOpponentName(string connectionId)
    {
        if (CurrentPlayerHost.GameConnectionId == connectionId)
        {
            return CurrentPlayerHost.UserName;
        }
        else
        {
            return CurrentPlayerGuest.UserName;
        }
    }
    public async Task<bool> IsTwoPlayersPlaying()
    {
        return await PlayerRepository.CheckIfTwoPlayersArePlaying(CurrentPlayerHost.Id, CurrentPlayerGuest.Id);
    }
}
