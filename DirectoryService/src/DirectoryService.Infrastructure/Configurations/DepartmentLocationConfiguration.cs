using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(dl => dl.DepartmentLocationId).HasName("pk_department_location");

        builder.Property(dl => dl.DepartmentLocationId).HasColumnName("department_location_id");

        builder.Property(dl => dl.DepartmentId)
            .HasColumnName("department_id");

        builder.Property(dl => dl.LocationId)
            .HasColumnName("location_id");

        builder.HasOne(dl => dl.Department)
            .WithMany(dl => dl.Locations)
            .HasForeignKey(dl => dl.DepartmentId);

        builder.HasOne(dl => dl.Location)
            .WithMany(l => l.Departments)
            .HasForeignKey(dl => dl.LocationId);
    }
}