using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Models;

public class PhotoScavengerHuntDbContext(DbContextOptions<PhotoScavengerHuntDbContext> options) : DbContext(options)
{
    public DbSet<PhotoSubmission> Photos => Set<PhotoSubmission>();
}