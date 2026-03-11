using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        builder.HasKey(l => l.Id).HasName("pk_location");

        builder.Property(l => l.LocationAddress)
            .HasConversion(l => l.Value, address => LocationAddress.Create(address).Value)
            .HasColumnName("location_address");
        builder.Property(l => l.Timezone)
            .HasConversion(l => l.Name, timezone => Timezone.Create(timezone).Value)
            .HasColumnName("timezone");

        builder.ComplexProperty(l => l.Name, nb =>
        {
            nb.Property(l => l.Value)
                .IsRequired()
                .HasColumnName("name");
        });

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("created_at");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("updated_at");

        builder.HasMany(l => l.Departments)
            .WithOne(d => d.Location)
            .HasForeignKey(d => d.LocationId);
    }
}