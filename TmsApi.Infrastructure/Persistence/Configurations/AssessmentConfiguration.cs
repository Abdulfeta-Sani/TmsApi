using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;
public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.MaxScore)
            .HasPrecision(6, 2);

        builder.Property(a => a.Weight)
            .HasPrecision(4, 2);

        builder.Property(a => a.CourseId)
            .IsRequired();

        builder.HasOne(a => a.Course)
            .WithMany(c => c.Assessments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}