using Microsoft.AspNetCore.Identity;
using TicTacToeGame.Domain.Enums;

namespace TicTacToeGame.Domain.Models
{
    public class Player : IdentityUser
    {
        public PlayerType PlayerType { get; set; }
        public string? Nickname { get; set; }
    }
}
