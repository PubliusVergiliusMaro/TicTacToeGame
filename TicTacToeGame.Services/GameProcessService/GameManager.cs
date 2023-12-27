using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;

namespace TicTacToeGame.Services.GameProcessService;

public class GameManager
{
    public Player CurrentPlayerHost;
    public Player CurrentPlayerGuest;
    public Player CurrentPlayer;

    public Game CurrentGame;
    public GamesHistory GamesHistory = new();
    public int HostWins = 0;
    public int GuestWins = 0;

    private AuthenticationState AuthenticationState;
    private ClaimsPrincipal? ClaimsPrincipal;

    public GameRepository GameRepository;
    public PlayerRepository PlayerRepository;

    public BoardElements[] Board = new BoardElements[TicTacToeRules.BOARD_SIZE];

    public bool IsInitialized = false;

    public async Task InitializeAuthState(AuthenticationStateProvider authenticationStateProvider)
    {
        AuthenticationState = await authenticationStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal = AuthenticationState.User;
    }
    public void InitializeRepositories(GameRepository gameRepository, PlayerRepository playerRepository)
    {
        GameRepository = gameRepository;
        PlayerRepository = playerRepository;
    }
    public string GetCurrentUserId()
    {
        return ClaimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
    public bool InitializeGame()
    {
        string currentUserId = GetCurrentUserId();
        CurrentGame = GameRepository.GetByUserId(currentUserId);
        if (CurrentGame == null)
        {
            return false;
        }
        return true;
    }
    
    public bool InitializePlayers()
    {
        if (CurrentGame == null)
        {
            return false;
        }

        CurrentPlayerHost = PlayerRepository.GetById(CurrentGame.PlayerHostId);
        CurrentPlayerGuest = PlayerRepository.GetById(CurrentGame.PlayerGuestId);
        CurrentPlayer = PlayerRepository.GetById(GetCurrentUserId());

        return true;
    }

    public void UpdateCurrentPlayerGameConnectionId(string gameConnectionId)
    {
        CurrentPlayer.GameConnectionId = gameConnectionId;

        PlayerRepository.UpdateEntity(CurrentPlayer);

        UpdateGuestOrHostGameConnectionId(gameConnectionId);
    }

    private void UpdateGuestOrHostGameConnectionId(string gameConnectionId)
    {
        if(CurrentPlayer.Id == CurrentPlayerHost.Id)
        {
            CurrentPlayerHost.GameConnectionId = gameConnectionId;
        }
        else
        {
            CurrentPlayerGuest.GameConnectionId = gameConnectionId;
        }
    }

    public bool IsAuthenticatedUser() => ClaimsPrincipal?.Identity?.IsAuthenticated == true;
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

    public bool IsTwoPlayersPlaying()
    {
        return PlayerRepository.CheckIfTwoPlayersArePlaying(CurrentPlayerHost.Id, CurrentPlayerGuest.Id);
    }
    public void ClearData()
    {
        AuthenticationState = null;
        ClaimsPrincipal = null;
        CurrentPlayerHost = null;
        CurrentPlayerGuest = null;
        CurrentPlayer = null;
        CurrentGame = null;
        GameRepository = null;
        PlayerRepository = null;
        IsInitialized = false;
        Board = new BoardElements[TicTacToeRules.BOARD_SIZE];
    }
}
