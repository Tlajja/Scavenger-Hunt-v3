using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Features.Photos;
using PhotoScavengerHunt.Features.Tasks;

public class PhotoScavengerHuntDbContext(DbContextOptions<PhotoScavengerHuntDbContext> options) : DbContext(options)
{
    public DbSet<PhotoSubmission> Photos => Set<PhotoSubmission>();
}