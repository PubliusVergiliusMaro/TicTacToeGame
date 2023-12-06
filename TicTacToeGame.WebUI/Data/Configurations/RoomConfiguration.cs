using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.WebUI.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder
           .Property(g => g.CreatedAt)
           .HasColumnType("datetime");

            builder
           .Property(g => g.UpdatedAt)
           .HasColumnType("datetime");
        }
    }
}
