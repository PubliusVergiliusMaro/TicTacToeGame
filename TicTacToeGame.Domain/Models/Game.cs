using Dapper.Contrib.Extensions;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    public class Game : EntityBase
    {
        public PlayerType? Winner { get; set; } = PlayerType.None;
        public GameState GameResult { get; set; } = GameState.Starting; // Maybe rename as gameState
        public PlayerType CurrentTurn { get; set; }//= Maybe add here rundomize or make in the manager
        
        public string PlayerHostId { get; set; }// in db int
        public string PlayerGuestId { get; set; }// in db int

        public int? GamesHistoryHostId { get; set; }// in db int
        public int? GamesHistoryGuestId { get; set; }// in db int
        
        public int RoomId { get; set; }
        [Write(false)]
        public Room? Room { get; set; }
    }
}
