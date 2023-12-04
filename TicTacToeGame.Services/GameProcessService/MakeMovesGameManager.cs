using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;

namespace TicTacToeGame.Services.GameProcessService;
public class MakeMovesGameManager : GameManagerBase
{

    private readonly IGamesStatisticsService _gamesStatisticsService;
    private HubConnection _connection;
    private readonly CheckForWinnerManager _checkForWinnerManager;
    private readonly GameRepository _gameRepository;

    public MakeMovesGameManager(AuthenticationStateProvider authenticationStateProvider,
        IGamesStatisticsService gamesStatisticsService, CheckForWinnerManager checkForWinnerManager, GameRepository gameRepository)
        : base(authenticationStateProvider)
    {
        _gamesStatisticsService = gamesStatisticsService;
        _checkForWinnerManager = checkForWinnerManager;
        _gameRepository = gameRepository;
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

                    // Only after 3 movements can a player win
                    await CheckForWinnerAfterMoves(board, CurrentGame);

                    // Switch player turns
                    await SentGameState(board, CurrentGame);
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

    private void PlaceMoveOnCell(int index, BoardElements[] board,Game CurrentGame)
    {
        board[index] = (CurrentGame.CurrentTurn == PlayerType.Host) ? BoardElements.X : BoardElements.O;
        Counter++;
    }

    private async Task CheckForWinnerAfterMoves(BoardElements[] board,Game CurrentGame)
    {
        if (Counter >= TicTacToeRules.MIN_COUNT_OF_MOVE_TO_WIN)
        {
            if (_checkForWinnerManager.CheckForWinner(board))
            {
                await FinishGameAndPutInHistory(CurrentGame);
            }
        }
    }

    private async Task FinishGameAndPutInHistory(Game CurrentGame)
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
            CurrentGame.Winner = CurrentGame.CurrentTurn;

            _gameRepository.UpdateEntity(CurrentGame);

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
        await _connection.SendAsync("SendGameStatus", CurrentGame.GameResult, GameStatus, CurrentGame.UniqueId);
    }

    private async Task SentGameState(BoardElements[] board,Game CurrentGame)
    {
        PlayerType nextPlayerTurn = (CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
        await _connection.SendAsync("SendGameState", board, nextPlayerTurn, CurrentGame.UniqueId);
    }

   
}
