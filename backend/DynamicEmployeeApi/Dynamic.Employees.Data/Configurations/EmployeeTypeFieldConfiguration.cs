using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Employees.Data.Configurations;

public class EmployeeTypeFieldConfiguration : IEntityTypeConfiguration<EmployeeTypeField>
{
    public void Configure(EntityTypeBuilder<EmployeeTypeField> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(e => e.EmployeeType)
            .WithMany(e => e.Fields)
            .HasForeignKey(f => f.EmployeeTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}