namespace TicTacToeGame.WebUI.BackgroundServices
{
    public class PlayerTrackingService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<PlayerTrackingService> _logger;
        private Timer? _timer = null;

        public PlayerTrackingService(ILogger<PlayerTrackingService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Player Tracking Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            _logger.LogInformation(
                "Player Tracking Service is working. Count: {Count}", count);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Player Tracking Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }


        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
