using Dapper.Contrib.Extensions;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    public class GamesHistory : EntityBase
    {
        public List<Game> Games { get; set; } = new List<Game>();
        public string PlayerId { get; set; }
        [Write(false)]
        public Player Player { get; set; }
    }
}
