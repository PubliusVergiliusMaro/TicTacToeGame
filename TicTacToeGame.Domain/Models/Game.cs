using Dapper.Contrib.Extensions;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    public class Game : EntityBase
    {
        public PlayerType? Winner { get; set; } = PlayerType.None;
        public GameState GameResult { get; set; } = GameState.Starting;
        public PlayerType CurrentTurn { get; set; }
        
        public string PlayerHostId { get; set; }
        public string PlayerGuestId { get; set; }

        public int? GamesHistoryHostId { get; set; }
        [Write(false)]
        public GamesHistory? GamesHistoryHost { get; set; }
        public int? GamesHistoryGuestId { get; set; }
        [Write(false)]
        public GamesHistory? GamesHistoryGuest { get; set; }
        
        public int? RoomId { get; set; }
        [Write(false)]
        public Room? Room { get; set; }
    }
}
