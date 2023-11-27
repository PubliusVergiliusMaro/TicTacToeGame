namespace TicTacToeGame.Domain.Models
{
    public class Room : EntityBase
    {
        public int ConnectionId { get; set; }
        public bool IsOpen { get; set; }
    }
        //public string Name { get; set; }
        //public string Password { get; set; }
        //public string Player1Id { get; set; }
        //public string Player2Id { get; set; }
        //public string WinnerId { get; set; }
        //public string LoserId { get; set; }
        //public string DrawId { get; set; }
        //public bool IsFull { get; set; }
        //public bool IsStarted { get; set; }
        //public bool IsFinished { get; set; }
        //public bool IsDraw { get; set; }
        //public bool IsAbandoned { get; set; }
        //public bool IsDeleted { get; set; }
        //public DateTime? StartedAt { get; set; }
        //public DateTime? FinishedAt { get; set; }
        //public DateTime? AbandonedAt { get; set; }
        //public DateTime? DeletedAt { get; set; }
        //public virtual Player Player1 { get; set; }
        //public virtual Player Player2 { get; set; }
        //public virtual Player Winner { get; set; }
        //public virtual Player Loser { get; set; }
        //public virtual Player Draw { get; set; }    
}
