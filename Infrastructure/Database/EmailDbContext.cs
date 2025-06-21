using Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class EmailDbContext : DbContext
{
    public DbSet<EmailMessage> EmailMessages { get; set; }

    public EmailDbContext(DbContextOptions<EmailDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.From).HasMaxLength(500);
            entity.Property(e => e.EmlContent).IsRequired();
        });
    }
}
