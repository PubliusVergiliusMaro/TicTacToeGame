using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<Player>(options)
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<GamesHistory> GamesHistory { get; set; }
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Game>()
                .HasOne(g => g.Room)
                .WithMany()
                .HasForeignKey(g => g.RoomId);

            builder.Entity<Game>()
                .HasOne<GamesHistory>()
                .WithMany(gh => gh.Games)
                .HasForeignKey(g => g.GamesHistoryHostId);

            builder.Entity<Game>()
                .HasOne<GamesHistory>()
                .WithMany(gh => gh.Games)
                .HasForeignKey(g => g.GamesHistoryGuestId);
        }
    }
}
