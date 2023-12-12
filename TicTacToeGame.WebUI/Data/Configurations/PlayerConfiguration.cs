using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Data.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Player> builder)
        {
            builder
            .Property(g => g.PasswordHash)
            .HasMaxLength(450);

            builder
            .Property(g => g.SecurityStamp)
            .HasMaxLength(450);

            builder
            .Property(g => g.ConcurrencyStamp)
            .HasMaxLength(450);

            builder
            .Property(g => g.PhoneNumber)
            .HasMaxLength(50);

            builder
            .Property(g => g.GameConnectionId)
            .HasMaxLength(450);

            builder
               .Property(g => g.UpdatedAt)
               .HasColumnType("datetime");
        }
    }
}
