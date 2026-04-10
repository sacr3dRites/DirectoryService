using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        builder.HasKey(p => p.Id).HasName("pk_position");

        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Name)
            .HasConversion(n => n.Value, v => CorrectPositionName.Create(v).Value)
            .IsRequired()
            .HasColumnName("name");

        builder.HasIndex(p => p.Name)
            .IsUnique()
            .HasFilter("is_active = true");

        builder.HasMany(p => p.Departments)
            .WithOne(d => d.Position)
            .HasForeignKey(d => d.PositionId);

        builder.Property(p => p.Description)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("updated_at");
    }
}