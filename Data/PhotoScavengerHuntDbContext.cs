using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Hubs;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Tasks;
using PhotoScavengerHunt.Features.Users;

public class PhotoScavengerHuntDbContext(DbContextOptions<PhotoScavengerHuntDbContext> options) : DbContext(options)
{
	public DbSet<HuntTask> Tasks => Set<HuntTask>();
	public DbSet<PhotoSubmission> Photos => Set<PhotoSubmission>();
	public DbSet<UserProfile> Users => Set<UserProfile>();
	public DbSet<Comment> Comments => Set<Comment>();
	public DbSet<Hub> Hubs => Set<Hub>();
	public DbSet<HubMember> HubMembers => Set<HubMember>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Comment>()
			.HasOne(c => c.PhotoSubmission)
			.WithMany(s => s.Comments)
			.HasForeignKey(c => c.PhotoSubmissionId)
			.OnDelete(DeleteBehavior.Cascade);

		// Hub relationships
		modelBuilder.Entity<HubMember>()
			.HasOne(hm => hm.Hub)
			.WithMany(h => h.Members)
			.HasForeignKey(hm => hm.HubId)
			.OnDelete(DeleteBehavior.Cascade);

		modelBuilder.Entity<HubMember>()
			.HasOne(hm => hm.User)
			.WithMany()
			.HasForeignKey(hm => hm.UserId)
			.OnDelete(DeleteBehavior.Restrict);

		// Ensure JoinCode is unique
		modelBuilder.Entity<Hub>()
			.HasIndex(h => h.JoinCode)
			.IsUnique();

		modelBuilder.Entity<HuntTask>().HasData(
			new HuntTask { Id = 1, Description = "Red car", Deadline = DateTime.Parse("2025-10-01"), Status = HuntTaskStatus.Open },
			new HuntTask { Id = 2, Description = "Blue mailbox", Deadline = DateTime.Parse("2025-10-02"), Status = HuntTaskStatus.Open }
		);

		modelBuilder.Entity<UserProfile>().HasData(
			new UserProfile { Id = 1, Name = "Ieva", Age = 20 },
			new UserProfile { Id = 2, Name = "Kristina", Age = 35 },
			new UserProfile { Id = 3, Name = "Ausra", Age = 40 },
			new UserProfile { Id = 4, Name = "Ula", Age = 61 }
		);

		modelBuilder.Entity<PhotoSubmission>().HasData(
			new PhotoSubmission { Id = 1, TaskId = 1, UserId = 1, PhotoUrl = "https://example.com/photo1.jpg", Votes = 5 },
			new PhotoSubmission { Id = 2, TaskId = 2, UserId = 2, PhotoUrl = "https://example.com/photo2.jpg", Votes = 3 }
		);
	}
}