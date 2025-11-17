using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ChallengeService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VotesService>();
builder.Services.AddScoped<PhotoSubmissionService>();

builder.Services.AddScoped<PhotoScavengerHunt.Repositories.IChallengeRepository, PhotoScavengerHunt.Repositories.ChallengeRepository>();
builder.Services.AddScoped<PhotoScavengerHunt.Repositories.IUserRepository, PhotoScavengerHunt.Repositories.UserRepository>();
builder.Services.AddScoped<PhotoScavengerHunt.Repositories.ITaskRepository, PhotoScavengerHunt.Repositories.TaskRepository>();
builder.Services.AddScoped<PhotoScavengerHunt.Repositories.IChallengeParticipantRepository, PhotoScavengerHunt.Repositories.ChallengeParticipantRepository>();
builder.Services.AddScoped<PhotoScavengerHunt.Repositories.IPhotoRepository, PhotoScavengerHunt.Repositories.PhotoRepository>();

builder.Services.AddDbContext<PhotoScavengerHuntDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();