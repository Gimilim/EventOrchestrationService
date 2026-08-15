using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings", "public");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasComment("ИД бронирования");

        builder.Property(b => b.EventId)
            .IsRequired()
            .HasComment("ИД события");

        builder.Property(b => b.UserId)
            .IsRequired()
            .HasComment("ИД пользователя");

        builder.Property(b => b.Status)
            .IsRequired()
            .HasColumnType("smallint")
            .HasConversion<short>()
            .HasComment("""
                        Статус бронирования:
                        1 - В обработке (Pending)
                        2 - Подтверждено (Confirmed)
                        3 - Отклонено (Rejected)
                        3 - Отменено (Cancelled)
                        9 - Ошибка обработки (Failed)
                        """);


        builder.Property(b => b.CreatedAt)
            .IsRequired()
            .HasComment("Дата создания бронирования");

        builder.Property(b => b.ProcessedAt)
            .HasComment("Дата обработки события");
    }
}