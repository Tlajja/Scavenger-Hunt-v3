using PhotoScavengerHunt.Repositories;
using PhotoScavengerHunt.Features.Challenges;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Services
{
    public class ChallengeDeadlineWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChallengeDeadlineWorker> _logger;

        public ChallengeDeadlineWorker(IServiceProvider serviceProvider, ILogger<ChallengeDeadlineWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var challengeRepo = scope.ServiceProvider.GetRequiredService<IChallengeRepository>();
                    var challengeService = scope.ServiceProvider.GetRequiredService<IChallengeService>();

                    var now = DateTime.UtcNow;
                    var challenges = await challengeRepo.GetAllAsync(publicOnly: false, sortBy: ChallengeSortBy.CreatedAtDesc);
                    
                    foreach (var c in challenges)
                    {
                        if (c.Status == ChallengeStatus.Open && c.SubmissionEndsAt.HasValue && now >= c.SubmissionEndsAt.Value)
                        {
                            _logger.LogInformation("Auto-advancing challenge {Id} to voting", c.Id);
                            c.Status = ChallengeStatus.Closed;
                        }
                        else if (c.Status == ChallengeStatus.Closed && c.VotingEndsAt.HasValue && now >= c.VotingEndsAt.Value)
                        {
                            _logger.LogInformation("Auto-finalizing challenge {Id}", c.Id);
                            await challengeService.FinalizeChallengeAsync(c.Id);
                        }
                    }
                    await challengeRepo.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ChallengeDeadlineWorker iteration failed");
                }
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}