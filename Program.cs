using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Services.Interfaces;
using PhotoScavengerHunt.Middleware;
using PhotoScavengerHunt.Features.Users;
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

builder.Services.AddSingleton<ActiveUsersService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddSignalR();

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

if (app.Environment.IsDevelopment())
{
 app.UseSwagger();
 app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("SignalR");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ActiveUsersHub>("/hubs/active-users");

app.Run();