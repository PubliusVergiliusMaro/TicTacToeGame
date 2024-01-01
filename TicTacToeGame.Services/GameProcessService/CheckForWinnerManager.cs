using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GamesStatisticServices;

namespace TicTacToeGame.Services.GameProcessService;
public class CheckForWinnerManager
{
    private readonly GameManager _gameManager;

    private readonly GamesStatisticsService _gamesStatisticsService;

    private readonly GameBoardManager _gameBoardManager;

    public event Action StateHasChanged;

    public string GameStatus { get; set; }

    public CheckForWinnerManager(GameManager gameManager,
        GamesStatisticsService gamesStatisticsService,
        GameBoardManager gameBoardManager)
    {
        _gameManager = gameManager;
        _gamesStatisticsService = gamesStatisticsService;
        _gameBoardManager=gameBoardManager;
    }

    public bool CheckForWinner() => CheckRowsForWinner() || CheckColumnsForWinner() || CheckDiagonalsForWinner();

    public bool CheckForTie()
    {
        if (_gameManager.Board.All(cell => cell != BoardElements.Empty))
        {
            GameStatus = "It's a tie!";
            StateHasChanged?.Invoke();
            return true;
        }
        return false;
    }
    private bool CheckRowsForWinner()
    {
        for (int row = TicTacToeRules.FIRST_ROW_OF_BOARD; row < TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS; row++)
        {
            if (AreAllEqual(_gameManager.Board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS],
                _gameManager.Board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS + 1], _gameManager.Board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS + 2]))
            {
                GameStatus = $"{_gameManager.Board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS]} wins!";
                return true;
            }
        }

        return false;
    }
    private bool CheckColumnsForWinner()
    {
        for (int col = TicTacToeRules.FIRST_COLUMN_OF_BOARD; col < TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS; col++)
        {
            if (AreAllEqual(_gameManager.Board[col], _gameManager.Board[col + TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS],
                _gameManager.Board[col + 2 * TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS]))
            {
                GameStatus = $"{_gameManager.Board[col]} wins!";
                return true;
            }
        }

        return false;
    }
    private bool CheckDiagonalsForWinner()
    {
        if (AreAllEqual(_gameManager.Board[TicTacToeRules.FIRST_ELEMENT_OF_DIAGONAL],
            _gameManager.Board[TicTacToeRules.SECOND_ELEMENT_OF_DIAGONAL], _gameManager.Board[TicTacToeRules.THIRD_ELEMENT_OF_DIAGONAL]))
        {
            GameStatus = $"{_gameManager.Board[TicTacToeRules.FIRST_ELEMENT_OF_DIAGONAL]} wins!";
            return true;
        }

        if (AreAllEqual(_gameManager.Board[TicTacToeRules.FIRST_ELEMENT_OF_REVERSE_DIAGONAL],
            _gameManager.Board[TicTacToeRules.SECOND_ELEMENT_OF_REVERSE_DIAGONAL], _gameManager.Board[TicTacToeRules.THIRD_ELEMENT_OF_REVERSE_DIAGONAL]))
        {
            GameStatus = $"{_gameManager.Board[TicTacToeRules.FIRST_ELEMENT_OF_REVERSE_DIAGONAL]} wins!";
            return true;
        }

        return false;
    }
    private bool AreAllEqual(BoardElements a, BoardElements b, BoardElements c)
    {
        return a != BoardElements.Empty && a == b && b == c;
    }

    public void ReceiveGameStatus(GameState receiveGameResult, string receiveGameStatus, PlayerType winner, Game game, int gameId)
    {
        _gameManager.CurrentGame = game;
        _gameManager.GameRepository.UpdateEntity(game);

        _gameManager.Board = _gameBoardManager.GetBoard(_gameManager.CurrentGame.UniqueId);

        GameStatus = receiveGameStatus;

        _gamesStatisticsService.GetGamesByResultAndPlayers(_gameManager);
        StateHasChanged?.Invoke();
    }
    public void ReceiveTestMessage(string userId)
    {
        if(_gameManager.CurrentPlayer.Id != userId)
        {
            int a = 10;
        }
    }

}
