using Dapper.Contrib.Extensions;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    [Table("dbo.GamesHistory")]
    public class GamesHistory : EntityBase
    {
        [Write(false)]
        public List<Game> Games { get; set; } = new List<Game>();

        public string PlayerId { get; set; }
        [Write(false)]
        public Player Player { get; set; }
    }
}
