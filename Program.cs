using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Middleware;
using PhotoScavengerHunt.Features.Users;
using PhotoScavengerHunt.Features.Photos;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVotesService, VotesService>();
builder.Services.AddScoped<IPhotoSubmissionService, PhotoSubmissionService>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();

builder.Services.AddSingleton<ActiveUsersService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddScoped<IChallengeRepository, ChallengeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IChallengeParticipantRepository, ChallengeParticipantRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();

builder.Services.AddDbContext<PhotoScavengerHuntDbContext>(options =>
 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalR", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PhotoScavengerHuntDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors("SignalR");
app.UseAuthorization();

app.MapControllers();
app.MapHub<ActiveUsersHub>("/hubs/active-users");
app.MapHub<CommentsHub>("/hubs/comments");

app.Run();