using System.Text.Json;
using System.Text.Json.Nodes;
using Dynamic.Employees.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dynamic.Employees.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    private const string FieldValuesColumnName = "FieldValuesJson";

    public void Configure(EntityTypeBuilder<Employee> builder)
    {
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

        builder.Property(e => e.FieldValuesJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

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
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                fieldValues => JsonSerializer.Serialize(fieldValues, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<JsonObject>(json, (JsonSerializerOptions?)null) ?? new JsonObject());
    }
}