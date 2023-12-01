namespace TicTacToeGame.Domain.Models
{
    public class Room : EntityBase
    {
        public int ConnectionId { get; set; }
        public bool IsOpen { get; set; }
    }
}
