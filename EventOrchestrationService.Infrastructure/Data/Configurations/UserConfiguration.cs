using EventOrchestrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventOrchestrationService.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "public");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasComment("ИД пользователя");

        builder.Property(u => u.Login)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Логин пользователя");

        builder.HasIndex(u => u.Login)
            .IsUnique();

        builder.Property(b => b.PasswordHash)
            .IsRequired()
            .HasComment("Хэш пароля");

        builder.Property(b => b.Role)
            .IsRequired()
            .HasColumnType("smallint")
            .HasConversion<short>()
            .HasComment("""
                        Роль Пользователя:
                        1 - Пользователь (User)
                        2 - Администратор (Admin)
                        """);

        builder.HasMany<Booking>(e => e.Bookings)
            .WithOne(b => b.User)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}