using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;

namespace TicTacToeGame.WebUI.BackgroundServices
{
    public class PlayerTrackingService : IHostedService, IDisposable
    {
        private int executionCount = 0;

        private const int SECONDS_BEFORE_DELETING_PLAYER = 120;

        private readonly PlayerRepository _playerRepository;

        private readonly GameReconnectingService _gameReconnectingService;

        private readonly ILogger<PlayerTrackingService> _logger;

        private readonly Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private Timer? _timer = null;

        public PlayerTrackingService(ILogger<PlayerTrackingService> logger, 
            PlayerRepository playerRepository, 
            GameReconnectingService gameReconnectingService)
        {
            _logger = logger;
            _playerRepository = playerRepository;
            _gameReconnectingService = gameReconnectingService;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Player Tracking Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(SECONDS_BEFORE_DELETING_PLAYER));

            return Task.CompletedTask;
        }
        private void AddNotActivePlayers()
        {
            List<Player> notActivePlayers = _playerRepository.GetAllNotActive();

            foreach (var player in notActivePlayers)
            {
                if (!_players.ContainsKey(player.Id))
                {
                    _players.Add(player.Id, player);
                }
            }
        }
        private void MakePlayersNotActive()
        {
            foreach (var player in _players)
            {
                //  зробити щоб брало знову той самий масив з бд і перевіряли чи є цей юзер в ньому

                Player currentPlayer = _playerRepository.GetById(player.Key);

                if(currentPlayer != null)
                {
                    if(currentPlayer.IsPlaying = true)
                    {
                        _gameReconnectingService.MakePlayerNotPlaying(player.Key);
                    }

                    _players.Remove(player.Key);
                }
            }
        }
        private void DoWork(object? state)
        {
            var iter = Interlocked.Increment(ref executionCount);

            MakePlayersNotActive();

            AddNotActivePlayers();

            _logger.LogInformation(
                "Player Tracking Service is working. Iteration - {Iteration}.Not active players count: {PlayerCount}", iter, _players.Count);
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
