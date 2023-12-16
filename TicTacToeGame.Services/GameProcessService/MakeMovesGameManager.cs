using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService;
public class MakeMovesGameManager
{
    private readonly GameRepository _gameRepository;

    private readonly GamesStatisticsService _gamesStatisticsService;

    private readonly GameReconnectingService _gameReconnectingService;

    private readonly CheckForWinnerManager _checkForWinnerManager;

    private readonly GameHubConnection _gameHubConnection;

    private readonly GameManager _gameManager;

    public string CurrentPlayerSign;

    public MakeMovesGameManager(AuthenticationStateProvider authenticationStateProvider,
        GamesStatisticsService gamesStatisticsService,
        CheckForWinnerManager checkForWinnerManager,
        GameRepository gameRepository,
        GameReconnectingService gameReconnectingService,
        GameHubConnection gameHubConnection,
        GameManager gameManager)
    {
        _gamesStatisticsService = gamesStatisticsService;
        _checkForWinnerManager = checkForWinnerManager;
        _gameRepository = gameRepository;
        _gameReconnectingService = gameReconnectingService;
        _gameHubConnection = gameHubConnection;
        _gameManager = gameManager;
    }

    public void InitializePlayers()
    {
        if (_gameManager.CurrentPlayer.Id == _gameManager.CurrentPlayerHost.Id)
        {
            CurrentPlayerSign = "X";
        }
        else
        {
            CurrentPlayerSign = "O";
        }
    }

    public async Task MakeMove(int index)
    {
        try
        {
            if (index < 0 || index >= _gameManager.Board.Length)
            {
                throw new IndexOutOfRangeException("Invalid index");
            }

            if (_gameManager.Board[index] == BoardElements.Empty && _gameManager.CurrentGame.GameResult == GameState.Starting)
            {
                // Check if it's the correct player's turn
                if (await IsCurrentPlayersTurn())
                {
                    // Put X or O on cell
                    PlaceMoveOnCell(index);
                    // Switch player turns
                    await SentGameState();
                    // Only after 3 movements can a player win
                    await CheckForWinnerAfterMoves();
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


    private async Task<bool> IsCurrentPlayersTurn()
    {
        return (_gameManager.CurrentGame.CurrentTurn == PlayerType.Host && _gameManager.ClaimsPrincipal.Claims.First().Subject.Name == _gameManager.CurrentPlayerHost.UserName) ||
               (_gameManager.CurrentGame.CurrentTurn == PlayerType.Guest && _gameManager.ClaimsPrincipal.Claims.First().Subject.Name == _gameManager.CurrentPlayerGuest.UserName);
    }

    private void PlaceMoveOnCell(int index)
    {
        _gameManager.Board[index] = DetermineWhoMakeMoveNow();
    }

    public BoardElements DetermineWhoMakeMoveNow() => (_gameManager.CurrentGame.CurrentTurn == PlayerType.Host) ? BoardElements.X : BoardElements.O;

    private async Task CheckForWinnerAfterMoves()
    {
        if (_checkForWinnerManager.CheckForWinner())
        {
            await FinishGameAndPutInHistory();
        }
        else if (_checkForWinnerManager.CheckForTie())
        {
            await FinishGameAndPutInHistory(true);
        }
    }

    private async Task FinishGameAndPutInHistory(bool isTie = false)
    {
        try
        {
            _gameManager.CurrentGame.GameResult = GameState.Finished;

            if (_gamesStatisticsService == null || _gameManager.CurrentPlayerHost == null || _gameManager.CurrentPlayerGuest == null)
            {
                // Handle the case where required objects are null
                throw new InvalidOperationException("One or more required objects are null.");
            }

            GamesHistory hostGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(_gameManager.CurrentPlayerHost.Id);
            GamesHistory guestGamesHistory = await _gamesStatisticsService.GetGamesHistoryByPlayerId(_gameManager.CurrentPlayerGuest.Id);

            if (hostGamesHistory == null || guestGamesHistory == null)
            {
                // Handle the case where games history is not found
                throw new InvalidOperationException("Games history not found for one or more players.");
            }

            _gameManager.CurrentGame.GamesHistoryHostId = hostGamesHistory.Id;
            _gameManager.CurrentGame.GamesHistoryGuestId = guestGamesHistory.Id;
            if (isTie)
            {
                _gameManager.CurrentGame.Winner = PlayerType.None;
            }
            else
                _gameManager.CurrentGame.Winner = (_gameManager.CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;

            _gameRepository.UpdateEntity(_gameManager.CurrentGame);
            await SendGameStatus(_checkForWinnerManager.GameStatus);

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerHost.Id);
            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerGuest.Id);

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

    private async Task SendGameStatus(string GameStatus)
    {
        await _gameHubConnection.SendGameStatus(_gameManager.CurrentGame.GameResult, GameStatus, (int)_gameManager.CurrentGame.RoomId);
    }

    private async Task SentGameState()
    {
        PlayerType nextPlayerTurn = (_gameManager.CurrentGame.CurrentTurn == PlayerType.Host) ? PlayerType.Guest : PlayerType.Host;
        int roomId = (int)_gameManager.CurrentGame.RoomId;

        await _gameHubConnection.SendGameState(_gameManager.Board, nextPlayerTurn, roomId);
    }

    public void UpdateGameAfterMove()
    {
        _gameRepository.UpdateEntity(_gameManager.CurrentGame);
    }
}
