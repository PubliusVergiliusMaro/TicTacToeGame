using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;

namespace TicTacToeGame.Services.GameProcessService;
public class MakeMovesGameManager : GameManagerBase
{

    private readonly GamesStatisticsService _gamesStatisticsService;
    private HubConnection _connection;
    private readonly CheckForWinnerManager _checkForWinnerManager;
    private readonly GameRepository _gameRepository;
    private readonly GameReconnectingService _gameReconnectingService;

    public MakeMovesGameManager(AuthenticationStateProvider authenticationStateProvider,
        GamesStatisticsService gamesStatisticsService,
        CheckForWinnerManager checkForWinnerManager,
        GameRepository gameRepository,
        GameReconnectingService gameReconnectingService)
        : base(authenticationStateProvider)
    {
        _gamesStatisticsService = gamesStatisticsService;
        _checkForWinnerManager = checkForWinnerManager;
        _gameRepository = gameRepository;
        _gameReconnectingService=gameReconnectingService;
    }
    public void InitializePlayers(Player PlayerHost, Player PlayerGuest, HubConnection hubConnection)
    {
        CurrentPlayerHost = PlayerHost;
        CurrentPlayerGuest = PlayerGuest;
        _connection = hubConnection;

    }
    public async Task MakeMove(int index, BoardElements[] board, Game CurrentGame, AuthenticationState authState, ClaimsPrincipal? user)
    {
        try
        {
            if (index < 0 || index >= board.Length)
            {
                throw new IndexOutOfRangeException("Invalid index");
            }

            if (board[index] == BoardElements.Empty && CurrentGame.GameResult == GameState.Starting)
            {
                // Check if it's the correct player's turn
                if (await IsCurrentPlayersTurn(CurrentGame, authState, user))
                {
                    // Put X or O on cell
                    PlaceMoveOnCell(index, board, CurrentGame);
                    // Switch player turns
                    await SentGameState(board, CurrentGame);
                    // Only after 3 movements can a player win
                    await CheckForWinnerAfterMoves(board, CurrentGame);
                }
            }
        }
        catch (IndexOutOfRangeException ex)
        {
            // Log or handle index out of range exception
            Console.WriteLine($"Index out of range: {ex.Message}");
            // You might want to throw or log here based on your application's needs
        }
        catch (NullReferenceException ex)
        {
            // Log or handle null reference exception
            Console.WriteLine($"Null reference: {ex.Message}");
            // You might want to throw or log here based on your application's needs
        }
        catch (Exception ex)
        {
            // Log or handle other exceptions
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            // You might want to throw or log here based on your application's needs
        }
    }


    private async Task<bool> IsCurrentPlayersTurn(Game CurrentGame, AuthenticationState authState, ClaimsPrincipal? user)
    {
        authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        user = authState.User;

        return (CurrentGame.CurrentTurn == PlayerType.Host && user.Claims.First().Subject.Name == CurrentPlayerHost.UserName) ||
               (CurrentGame.CurrentTurn == PlayerType.Guest && user.Claims.First().Subject.Name == CurrentPlayerGuest.UserName);
    }

    private void PlaceMoveOnCell(int index, BoardElements[] board, Game CurrentGame)
    {
        board[index] = DetermineWhoMakeMoveNow(CurrentGame);
    }

    public BoardElements DetermineWhoMakeMoveNow(Game CurrentGame) => (CurrentGame.CurrentTurn == PlayerType.Host) ? BoardElements.X : BoardElements.O;

    private async Task CheckForWinnerAfterMoves(BoardElements[] board, Game CurrentGame)
    {   
        if (_checkForWinnerManager.CheckForWinner(board))
        {
            await FinishGameAndPutInHistory(CurrentGame);
        }
        else if (_checkForWinnerManager.CheckForTie(board))
        {
            await FinishGameAndPutInHistory(CurrentGame, true);
        }
    }
    public bool CheckForTie(BoardElements[] board)
    {
        return !board.Contains(BoardElements.Empty);
    }

    private async Task FinishGameAndPutInHistory(Game CurrentGame, bool isTie = false)
    {
        try
        {
            CurrentGame.GameResult = GameState.Finished;

            if (_gamesStatisticsService == null || CurrentPlayerHost == null || CurrentPlayerGuest == null)
            {
                // Handle the case where required objects are null
                throw new InvalidOperationException("One or more required objects are null.");
            }

            GamesHistory hostGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(CurrentPlayerHost.Id);
            GamesHistory guestGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(CurrentPlayerGuest.Id);

            if (hostGamesHistory == null || guestGamesHistory == null)
            {
                // Handle the case where games history is not found
                throw new InvalidOperationException("Games history not found for one or more players.");
            }

            CurrentGame.GamesHistoryHostId = hostGamesHistory.Id;
            CurrentGame.GamesHistoryGuestId = guestGamesHistory.Id;
            if (isTie)
            {
                CurrentGame.Winner = PlayerType.None;
            }
            else
                CurrentGame.Winner = CurrentGame.CurrentTurn;

            _gameRepository.UpdateEntity(CurrentGame);

            _gameReconnectingService.MakePlayerNotPlaying(CurrentPlayerHost.Id);
            _gameReconnectingService.MakePlayerNotPlaying(CurrentPlayerGuest.Id);

            await SendGameStatus(_checkForWinnerManager.GameStatus, CurrentGame);
        }
        catch (NullReferenceException ex)
        {
            // Handle NullReferenceException
            Console.WriteLine($"NullReferenceException: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // Handle InvalidOperationException
            Console.WriteLine($"InvalidOperationException: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            // Handle DbUpdateException
            Console.WriteLine($"DbUpdateException: {ex.Message}");
        }
        catch (DataException ex)
        {
            // Handle DataException
            Console.WriteLine($"DataException: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle any other unexpected exceptions
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }

    private async Task SendGameStatus(string GameStatus, Game CurrentGame)
    {
        await _connection.SendAsync("SendGameStatus", CurrentGame.GameResult, GameStatus, CurrentGame.RoomId);
    }

    private async Task SentGameState(BoardElements[] board, Game CurrentGame)
    {
        PlayerType nextPlayerTurn = (CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
        await _connection.SendAsync("SendGameState", board, nextPlayerTurn, CurrentGame.RoomId);
    }

    public void UpdateGameAfterMove(Game game)
    {
        _gameRepository.UpdateEntity(game);
    }
}
