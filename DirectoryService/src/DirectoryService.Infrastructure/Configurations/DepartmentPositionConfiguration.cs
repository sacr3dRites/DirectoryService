using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        builder.HasKey(dp => dp.DepartmentPositionId).HasName("pk_department_position");

        builder.Property(dp => dp.DepartmentPositionId).HasColumnName("department_position_id");

        builder.Property(dp => dp.DepartmentId)
            .HasColumnName("department_id");

        builder.Property(dp => dp.PositionId)
            .HasColumnName("position_id");

        builder.HasOne(dp => dp.Department)
            .WithMany(dp => dp.Positions)
            .HasForeignKey(dp => dp.DepartmentId);

        builder.HasOne(dp => dp.Position)
            .WithMany()
            .HasForeignKey(dp => dp.PositionId);
    }
}