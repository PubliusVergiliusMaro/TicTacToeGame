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

            builder.ApplyConfiguration(new Configurations.GameConfiguration());
            builder.ApplyConfiguration(new Configurations.GamesHistoryConfiguration());
            builder.ApplyConfiguration(new Configurations.PlayerConfiguration());
            builder.ApplyConfiguration(new Configurations.RoomConfiguration());
        }
    }
}
