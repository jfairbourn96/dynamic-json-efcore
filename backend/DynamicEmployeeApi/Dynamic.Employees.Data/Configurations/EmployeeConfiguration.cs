using Dynamic.Employees.Core.Models;
using Dynamic.Json.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Employees.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    private const string FieldValuesColumnName = "FieldValuesJson";

    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable(nameof(Employee));
        
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Department)
            .HasMaxLength(100);

        builder.HasOne(e => e.EmployeeType)
            .WithMany()
            .HasForeignKey(e => e.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.FirstName);
        builder.HasIndex(e => e.LastName);
        builder.HasIndex(e => e.EmployeeTypeId);

        builder.Property(e => e.FieldValues)
            .HasColumnName(FieldValuesColumnName)
            .HasJsonConversion();
    }
}