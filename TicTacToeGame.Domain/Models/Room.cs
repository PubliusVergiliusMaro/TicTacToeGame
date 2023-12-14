using Dapper.Contrib.Extensions;

namespace TicTacToeGame.Domain.Models
{
    public class Room : EntityBase
    {
        public int ConnectionId { get; set; }
        public bool IsOpen { get; set; }

        [Write(false)]
        public List<Game> Game { get; set; } = new();
    }
}
