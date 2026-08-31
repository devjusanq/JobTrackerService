using JobTrackerService.Jobs.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerService.Jobs.Infrastructure;

public sealed class JobsDbContext(DbContextOptions<JobsDbContext> options)
    : DbContext(options), JobTrackerService.Jobs.Application.IUnitOfWork
{
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobPhoto> JobPhotos => Set<JobPhoto>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("jobs");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobsDbContext).Assembly);
    }
}

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.OwnsOne(x => x.Address, address => address.ToTable("jobs"));
        builder.Metadata.FindNavigation(nameof(Job.Photos))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(x => x.Photos).WithOne().HasForeignKey("JobId").OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class JobPhotoConfiguration : IEntityTypeConfiguration<JobPhoto>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<JobPhoto> builder)
    { builder.ToTable("job_photos"); builder.HasKey(x => x.Id); builder.Property<Guid>("JobId").IsRequired(); }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error { get; set; }
}