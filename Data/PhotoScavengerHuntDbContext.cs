using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Users;

public class PhotoScavengerHuntDbContext : DbContext
{
    public PhotoScavengerHuntDbContext(DbContextOptions<PhotoScavengerHuntDbContext> options) : base(options)
    {
    }

    public DbSet<HuntTask> Tasks => Set<HuntTask>();
    public DbSet<PhotoSubmission> Photos => Set<PhotoSubmission>();
    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeParticipant> ChallengeParticipants => Set<ChallengeParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.PhotoSubmission)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.PhotoSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChallengeParticipant>()
            .HasOne(cp => cp.Challenge)
            .WithMany(c => c.Participants)
            .HasForeignKey(cp => cp.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChallengeParticipant>()
            .HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HuntTask>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<HuntTask>()
            .Property(t => t.Deadline)
            .IsRequired(false);

        // Seed with deterministic timestamps to avoid EF PendingModelChangesWarning
        var seedCreatedAt1 = new DateTime(2025,1,1,0,0,0, DateTimeKind.Utc);
        var seedCreatedAt2 = new DateTime(2025,1,2,0,0,0, DateTimeKind.Utc);

        modelBuilder.Entity<HuntTask>().HasData(
            new HuntTask { Id = 1, Description = "Red car", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = 0 },
            new HuntTask { Id = 2, Description = "Blue mailbox", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = 0 }
        );

        modelBuilder.Entity<UserProfile>().HasData(
            new UserProfile { Id = 1, Name = "Ieva" },
            new UserProfile { Id = 2, Name = "Kristina" },
            new UserProfile { Id = 3, Name = "Ausra" },
            new UserProfile { Id = 4, Name = "Ula" }
        );

        modelBuilder.Entity<PhotoSubmission>().HasData(
            new PhotoSubmission { Id = 1, TaskId = 1, UserId = 1, PhotoUrl = "https://example.com/photo1.jpg", Votes = 5 },
            new PhotoSubmission { Id = 2, TaskId = 2, UserId = 2, PhotoUrl = "https://example.com/photo2.jpg", Votes = 3 }
        );
    }
}