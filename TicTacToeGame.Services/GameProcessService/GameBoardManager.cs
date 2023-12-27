using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameBoardManager
    {
        private readonly Dictionary<string, BoardElements[]> GameBoard = new Dictionary<string, BoardElements[]>();


        public void AddBoard(string gameUniqueId, BoardElements[] board)
        {
            GameBoard.Add(gameUniqueId, board);
        }

        public BoardElements[] GetBoard(string gameUniqueId)
        {
            return GameBoard[gameUniqueId];
        }

        public void RemoveBoard(string gameUniqueId)
        {
            GameBoard.Remove(gameUniqueId);
        }
    }
}
