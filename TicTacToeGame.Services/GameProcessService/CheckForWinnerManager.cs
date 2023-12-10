using Polly.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Services.GameProcessService;
public class CheckForWinnerManager
{
    public string GameStatus = "";
    public bool CheckForWinner(BoardElements[] board)
    {
        if (CheckRowsForWinner(board) || CheckColumnsForWinner(board) || CheckDiagonalsForWinner(board))
        {
            return true;
        }

        return false;
    }
    public bool CheckForTie(BoardElements[] board)
    {
        // Check for a tie
        if (board.All(cell => cell != BoardElements.Empty))
        {
            GameStatus = "It's a tie!";
            return true;
        }
        return false;
    }
    private bool CheckRowsForWinner(BoardElements[] board)
    {
        for (int row = TicTacToeRules.FIRST_ROW_OF_BOARD; row < TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS; row++)
        {
            if (AreAllEqual(board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS],
                board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS + 1], board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS + 2]))
            {
                GameStatus = $"{board[row * TicTacToeRules.MAX_COUNT_OF_BOARD_ROWS]} wins!";
                return true;
            }
        }

        return false;
    }
    private bool CheckColumnsForWinner(BoardElements[] board)
    {
        for (int col = TicTacToeRules.FIRST_COLUMN_OF_BOARD; col < TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS; col++)
        {
            if (AreAllEqual(board[col], board[col + TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS],
                board[col + 2 * TicTacToeRules.MAX_COUNT_OF_BOARD_COLUMNS]))
            {
                GameStatus = $"{board[col]} wins!";
                return true;
            }
        }

        return false;
    }
    private bool CheckDiagonalsForWinner(BoardElements[] board)
    {
        if (AreAllEqual(board[TicTacToeRules.FIRST_ELEMENT_OF_DIAGONAL],
            board[TicTacToeRules.SECOND_ELEMENT_OF_DIAGONAL], board[TicTacToeRules.THIRD_ELEMENT_OF_DIAGONAL]))
        {
            GameStatus = $"{board[TicTacToeRules.FIRST_ELEMENT_OF_DIAGONAL]} wins!";
            return true;
        }

        if (AreAllEqual(board[TicTacToeRules.FIRST_ELEMENT_OF_REVERSE_DIAGONAL],
            board[TicTacToeRules.SECOND_ELEMENT_OF_REVERSE_DIAGONAL], board[TicTacToeRules.THIRD_ELEMENT_OF_REVERSE_DIAGONAL]))
        {
            GameStatus = $"{board[TicTacToeRules.FIRST_ELEMENT_OF_REVERSE_DIAGONAL]} wins!";
            return true;
        }

        return false;
    }
    private bool AreAllEqual(BoardElements a, BoardElements b, BoardElements c)
    {
        return a != BoardElements.Empty && a == b && b == c;
    }
}
