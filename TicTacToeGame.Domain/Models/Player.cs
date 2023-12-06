using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using TicTacToeGame.Domain.Enums;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

namespace TicTacToeGame.Domain.Models
{
    [Table("dbo.AspNetUsers")]
    public class Player : IdentityUser
    {
        public PlayerType PlayerType { get; set; }
        public string? Nickname { get; set; }
        public string? GameConnectionId { get; set;}

        [Write(false)]
        public GamesHistory GamesHistory { get; set; }
    }
}
