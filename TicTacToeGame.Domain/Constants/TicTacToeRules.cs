using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeGame.Domain.Constants;
public class TicTacToeRules
{
    public const int BOARD_SIZE = 9;
    public const int MIN_COUNT_OF_MOVE_TO_WIN = 3;
    public const int MAX_COUNT_OF_BOARD_ROWS = 3;
    public const int FIRST_ROW_OF_BOARD = 0;
    public const int MAX_COUNT_OF_BOARD_COLUMNS = 3;
    public const int FIRST_COLUMN_OF_BOARD = 0;
    public const int FIRST_ELEMENT_OF_DIAGONAL = 0;
    public const int SECOND_ELEMENT_OF_DIAGONAL = 4;
    public const int THIRD_ELEMENT_OF_DIAGONAL = 8;
    public const int FIRST_ELEMENT_OF_REVERSE_DIAGONAL = 2;
    public const int SECOND_ELEMENT_OF_REVERSE_DIAGONAL = 4;
    public const int THIRD_ELEMENT_OF_REVERSE_DIAGONAL = 6;
    public const int MIN_COUNT_MOVES_TO_WIN = 5;
}
