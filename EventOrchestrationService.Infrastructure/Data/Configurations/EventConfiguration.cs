using EventOrchestrationService.Domain.Constants;
using EventOrchestrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventOrchestrationService.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", "public");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasComment("ИД события");

        builder.Property(b => b.Title)
            .IsRequired()
            .HasComment("Название события")
            .HasMaxLength(EventConstants.MaxTitleLength);

        builder.Property(b => b.Description)
            .HasComment("Описание события")
            .HasMaxLength(EventConstants.MaxDescriptionLength);

        builder.Property(b => b.StartAt)
            .IsRequired()
            .HasComment("Дата начала события");

        builder.Property(b => b.EndAt)
            .IsRequired()
            .HasComment("Дата окончания события");

        builder.Property(b => b.TotalSeats)
            .IsRequired()
            .HasComment("Общее количество мест на событие");

        builder.Property(b => b.AvailableSeats)
            .HasComment("Доступное количество мест на событие");

        builder.HasMany<Booking>(e => e.Bookings)
            .WithOne(b => b.Event)
            .HasForeignKey(b => b.EventId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}