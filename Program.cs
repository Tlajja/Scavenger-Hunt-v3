using Microsoft.EntityFrameworkCore;
using PhotoScavengerHunt.Services;
using PhotoScavengerHunt.Features.Users;
using Microsoft.AspNetCore.SignalR;

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

app.UseCors("SignalR");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ActiveUsersHub>("/hubs/active-users");

app.Run();