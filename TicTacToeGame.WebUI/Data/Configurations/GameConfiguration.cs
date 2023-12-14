using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Data.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder
            .Property(g => g.PlayerHostId)
            .HasMaxLength(450);

            builder
            .Property(g => g.PlayerGuestId)
            .HasMaxLength(450);

            builder
            .Property(g => g.CreatedAt)
            .HasColumnType("datetime");

            builder
           .Property(g => g.UpdatedAt)
           .HasColumnType("datetime");


            builder
                .HasOne(g => g.Room)
                .WithMany(r => r.Game)
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<GamesHistory>()
                .WithMany(gh => gh.Games)
                .HasForeignKey(g => g.GamesHistoryHostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<GamesHistory>()
                .WithMany(gh => gh.Games)
                .HasForeignKey(g => g.GamesHistoryGuestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
