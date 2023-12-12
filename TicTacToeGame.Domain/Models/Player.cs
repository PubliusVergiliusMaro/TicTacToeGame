using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using TicTacToeGame.Domain.Enums;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

namespace TicTacToeGame.Domain.Models
{
    [Table("dbo.AspNetUsers")]
    public class Player : IdentityUser
    {
        public string? GameConnectionId { get; set;}
        public bool IsPlaying { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        [Write(false)]
        public GamesHistory GamesHistory { get; set; }
    }
}
