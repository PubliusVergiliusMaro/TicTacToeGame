using Microsoft.EntityFrameworkCore;
using System.Data;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Services.GamesStatisticServices;
using TicTacToeGame.Services.HubConnections;

namespace TicTacToeGame.Services.GameProcessService;
public class MakeMovesGameManager
{
    private readonly GamesStatisticsService _gamesStatisticsService;

    private readonly GameReconnectingService _gameReconnectingService;

    private readonly CheckForWinnerManager _checkForWinnerManager;

    private readonly GameBoardManager _gameBoardManager;

    private GameHubConnection _gameHubConnection;

    private readonly GameManager _gameManager;

    public string CurrentPlayerSign;

    public event Action StateHasChanged;

    public bool MoveWasMade { get; set; } = false;
    public MakeMovesGameManager(GamesStatisticsService gamesStatisticsService,
        CheckForWinnerManager checkForWinnerManager,
        GameReconnectingService gameReconnectingService,
        GameHubConnection gameHubConnection,
        GameBoardManager gameBoardManager,
        GameManager gameManager)
    {
        _gamesStatisticsService = gamesStatisticsService;
        _checkForWinnerManager = checkForWinnerManager;
        _gameReconnectingService = gameReconnectingService;
        _gameHubConnection = gameHubConnection;
        _gameManager = gameManager;
        _gameBoardManager = gameBoardManager;
    }
    public void SetHubConnection(GameHubConnection gameHubConnection)
    {
        _gameHubConnection = gameHubConnection;
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
            if (MoveWasMade)
            {
                return;
            }

            MoveWasMade = true;

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
                    if (_gameManager.Board.Count(cell => cell == BoardElements.Empty)<TicTacToeRules.MIN_COUNT_MOVES_TO_WIN)
                    {
                        await CheckForWinnerAfterMoves();
                    }
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
        bool isCurrentHostTurn = _gameManager.CurrentGame.CurrentTurn == PlayerType.Host;
        bool isCurrentGuestTurn = _gameManager.CurrentGame.CurrentTurn == PlayerType.Guest;

        bool isCurrentPlayerHost = _gameManager.CurrentPlayer.Id == _gameManager.CurrentPlayerHost.Id;
        bool isCurrentPlayerGuest = _gameManager.CurrentPlayer.Id == _gameManager.CurrentPlayerGuest.Id;

        return (isCurrentHostTurn && isCurrentPlayerHost) || (isCurrentGuestTurn && isCurrentPlayerGuest);
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

            _gameManager.GameRepository.UpdateEntity(_gameManager.CurrentGame);
            await SendGameStatus(_checkForWinnerManager.GameStatus);

            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerHost.Id);
            _gameManager.CurrentPlayerHost.IsPlaying = false;
            _gameReconnectingService.MakePlayerNotPlaying(_gameManager.CurrentPlayerGuest.Id);
            _gameManager.CurrentPlayerGuest.IsPlaying = false;

            _gamesStatisticsService.UpdatePlayersGameHistory(_gameManager.CurrentPlayerHost.Id, _gameManager.CurrentPlayerGuest.Id, _gameManager.CurrentGame.RoomId);

            _gameBoardManager.RemoveBoard(_gameManager.CurrentGame.UniqueId);

            StateHasChanged?.Invoke();
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

        _gameBoardManager.UpdateBoard(_gameManager.CurrentGame.UniqueId, _gameManager.Board);

        await _gameHubConnection.SendGameState(_gameManager.Board, nextPlayerTurn, (int)_gameManager.CurrentGame.RoomId);
    }

    public void UpdateGameAfterMove()
    {
        _gameManager.GameRepository.UpdateEntity(_gameManager.CurrentGame);
    }

    public void ReceiveAnotherPlayerBoard(string userId, BoardElements[] playerBoard)
    {
        if (userId != _gameManager.CurrentPlayer.Id)
        {
            _gameManager.Board = playerBoard;
            StateHasChanged?.Invoke();
        }
    }
    public void AskToReceiveAnotherPlayerBoard(string userId, int gameId)
    {
        AskAnotherPlayerBoardAsync(userId, gameId).GetAwaiter().GetResult();
    }
    private async Task AskAnotherPlayerBoardAsync(string userId, int gameId)
    {
        if (_gameManager.CurrentPlayer != null && _gameManager.CurrentPlayer.Id != userId)
        {
            if (!_gameManager.IsLoadingNextGame)
            {
                await _gameHubConnection.SendAnotherPlayerBoard((int)_gameManager.CurrentGame.RoomId, _gameManager.CurrentPlayer.Id, _gameManager.Board);
            }
            else
            {
                Game newGame = _gameManager.GameRepository.GetByUserId(_gameManager.GetCurrentUserId());

                await _gameHubConnection.SendAnotherPlayerBoard((int)newGame.RoomId, _gameManager.CurrentPlayer.Id, new BoardElements[TicTacToeRules.BOARD_SIZE]);
            }
        }
    }

    public void ReceiveGameState(BoardElements[] receivedBoard, PlayerType nextPlayerTurn, int gameId)
    {
        _gameManager.Board = receivedBoard;
        _gameManager.CurrentGame.CurrentTurn = nextPlayerTurn;
        UpdateGameAfterMove();
        MoveWasMade = false;
        StateHasChanged?.Invoke();
    }
}
