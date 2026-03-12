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

        builder.ComplexProperty(p => p.Name, nb =>
        {
            nb.Property(p => p.Value)
                .IsRequired()
                .HasColumnName("name");
        });

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