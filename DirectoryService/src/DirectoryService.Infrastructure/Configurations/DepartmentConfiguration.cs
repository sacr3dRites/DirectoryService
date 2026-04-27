using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id).HasName("pk_department");

        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.Identifier)
            .HasConversion(d => d.Value, identifier => DepartmentIdentifier.Create(identifier).Value)
            .HasColumnName("identifier")
            .IsRequired();

        builder.HasIndex(d => d.Identifier)
            .IsUnique()
            .HasDatabaseName("idx_department_identifier");

        builder.ComplexProperty(d => d.Path, nb =>
        {
            nb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("path");
        });

        builder.ComplexProperty(d => d.Name, nb =>
        {
            nb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("name");
        });

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("timezone('utc',now())")
            .HasColumnName("updated_at");

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasColumnName("is_active");


        builder.HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey("parent_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.Depth)
            .IsRequired()
            .HasColumnName("depth");

        builder.HasMany(d => d.Locations)
            .WithOne(l => l.Department)
            .HasForeignKey(l => l.DepartmentId);

        builder.HasMany(d => d.Positions)
            .WithOne(p => p.Department)
            .HasForeignKey(p => p.DepartmentId);
    }
}