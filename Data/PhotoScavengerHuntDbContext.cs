using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Features.Photos;
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
    public DbSet<ChallengeTask> ChallengeTasks => Set<ChallengeTask>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<CommentReaction> CommentReactions => Set<CommentReaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.PhotoSubmission)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.PhotoSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentReaction>()
            .HasOne(cr => cr.Comment)
            .WithMany(c => c.Reactions)
            .HasForeignKey(cr => cr.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentReaction>()
            .HasIndex(cr => new { cr.CommentId, cr.UserId, cr.Emoji })
            .IsUnique();

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.PhotoSubmission)
            .WithMany()
            .HasForeignKey(v => v.PhotoSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.PhotoSubmissionId, v.UserId })
            .IsUnique();

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

        // Configure join entity
        modelBuilder.Entity<ChallengeTask>()
            .HasKey(ct => new { ct.ChallengeId, ct.TaskId });
        modelBuilder.Entity<ChallengeTask>()
            .HasOne(ct => ct.Challenge)
            .WithMany(c => c.ChallengeTasks)
            .HasForeignKey(ct => ct.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChallengeTask>()
            .HasOne(ct => ct.Task)
            .WithMany() // tasks can belong to many challenges
            .HasForeignKey(ct => ct.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HuntTask>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql("datetime('now')");

        modelBuilder.Entity<HuntTask>()
            .Property(t => t.Deadline)
            .IsRequired(false);

        var seedCreatedAt1 = new DateTime(2025,1,1,0,0,0, DateTimeKind.Utc);
        var seedCreatedAt2 = new DateTime(2025,1,2,0,0,0, DateTimeKind.Utc);

        modelBuilder.Entity<HuntTask>().HasData(
            new HuntTask { Id = 1, Description = "Red car", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 2, Description = "Blue mailbox", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 3, Description = "Clock", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 4, Description = "Reflection in water or glass", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 5, Description = "Bench with a view", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 6, Description = "Fire hydrant", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 7, Description = "Something with wheels (not a car)", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 8, Description = "Something perfectly symmetrical", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 9, Description = "Tree with colorful leaves", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 10, Description = "Animal or pet (no humans visible)", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 11, Description = "Flower growing in an unexpected place", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 12, Description = "Cloud that looks like something", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 13, Description = "Interesting rock or stone", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 14, Description = "Door with a vibrant color", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 15, Description = "Park", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 16, Description = "Interesting street art or mural", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 17, Description = "Building with more than 10 floors", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 18, Description = "Statue", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 19, Description = "Street sign with an interesting name", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 20, Description = "Warning or caution sign", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 21, Description = "Advertisement with an animal", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 22, Description = "Sign in a language other than your native one", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 23, Description = "Bicycle with a basket", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 24, Description = "Vehicle with a funny bumper sticker", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 25, Description = "Electric vehicle or charging station", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 26, Description = "Public transportation (bus, train, tram)", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 27, Description = "House number that adds up to 10", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 28, Description = "Something with stripes", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 29, Description = "Three items of the same color in one photo", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 30, Description = "Perfect circle in nature or architecture", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 31, Description = "Coffee shop", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 32, Description = "Something yellow you can eat", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 33, Description = "Ice cream shop", CreatedAt = seedCreatedAt1, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 34, Description = "Bakery", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 38, Description = "Flag flying in the wind", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 },
            new HuntTask { Id = 40, Description = "Rainbow", CreatedAt = seedCreatedAt2, Deadline = null, AuthorId = -1 }
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