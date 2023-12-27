using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Services.GameProcessService
{
    public class GameBoardManager
    {
        private readonly Dictionary<Guid, BoardElements[]> GameBoard = new();

        public void AddBoardIfNotExist(Guid gameUniqueId, BoardElements[] board)
        {
            try
            {
                if (!GameBoard.ContainsKey(gameUniqueId))
                {
                    GameBoard.Add(gameUniqueId, board);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateBoard(Guid gameUniqueId, BoardElements[] board)
        {
            try
            {
                if (GameBoard.ContainsKey(gameUniqueId))
                    GameBoard[gameUniqueId] = board;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public BoardElements[] GetBoard(Guid gameUniqueId)
        {
            try
            {
                if (GameBoard.ContainsKey(gameUniqueId))
                {
                    return GameBoard[gameUniqueId];
                }
                else
                    throw new Exception("Board not found");
                   
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void RemoveBoard(Guid gameUniqueId)
        {
            try
            {
                if (GameBoard.ContainsKey(gameUniqueId))
                {
                    GameBoard.Remove(gameUniqueId);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
