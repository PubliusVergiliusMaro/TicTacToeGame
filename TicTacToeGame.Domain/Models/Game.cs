using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    public class Game : EntityBase
    {
        public PlayerType Winner { get; set; } = PlayerType.None;
        public GameResult GameResult { get; set; } = GameResult.NotFinished;
        public PlayerType CurrentTurn { get; set; } //= Maybe add here rundomize or make in the manager
        public string PlayerHostId { get; set; }// in db int
        public string PlayerGuestId { get; set; }// in db int
        public int GamesHistoryId { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }
    }
}
