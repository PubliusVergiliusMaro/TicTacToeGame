using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Data.Configurations
{
    public class GamesHistoryConfiguration : IEntityTypeConfiguration<GamesHistory>
    {
        public void Configure(EntityTypeBuilder<GamesHistory> builder)
        {
            builder
           .Property(g => g.CreatedAt)
           .HasColumnType("datetime");

            builder
           .Property(g => g.UpdatedAt)
           .HasColumnType("datetime");

            builder
            .HasOne<Player>(gh => gh.Player)
            .WithOne(Player => Player.GamesHistory)
            .HasForeignKey<GamesHistory>(gh => gh.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
