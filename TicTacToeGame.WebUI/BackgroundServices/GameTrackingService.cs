
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;

namespace TicTacToeGame.WebUI.BackgroundServices
{
    public class GameTrackingService : IHostedService, IDisposable
    {
        private int executionCount = 0;

        private readonly ILogger<GameTrackingService> _logger;

        private Timer? _timer = null;

        private readonly GameRepository _gameRepository;

        private readonly Dictionary<Game, TimeSpan> EmptyGames = new();

        private readonly GameBoardManager _gameBoardManager;

        public GameTrackingService(ILogger<GameTrackingService> logger, GameRepository gameRepository, GameBoardManager gameBoardManager)
        {
            _logger = logger;
            _gameRepository = gameRepository;
            _gameBoardManager = gameBoardManager;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Game Tracking Service running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(GameTrackingConstants.SECONDS_BEFORE_DELETING_GAME));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            ClearEmptyGames();

            List<Game> games = _gameRepository.GetEmptyGames();

            foreach (var game in games)
            {
                AddEmptyGame(game);
            }

            // Remove
            _logger.LogError("Game Tracking Service is working. Iteration: {Count}. Number of games with no players {EmptyGames}", count, EmptyGames.Count);
            //

            _logger.LogInformation("Game Tracking Service is working. Iteration: {Count}. Number of games with no players {EmptyGames}", count, EmptyGames.Count);
        }

        private void ClearEmptyGames()
        {
            foreach (var emptyGame in EmptyGames)
            {
                // може легше буде зробити щоб брало знову той самий масив з бд і перевіряли чи є цей юзер в ньому
                Game currentEmptyGame = _gameRepository.GetById(emptyGame.Key.Id);
                
                if (currentEmptyGame != null)
                {
                    if (currentEmptyGame.GameResult == GameState.Starting)
                    {
                        _gameRepository.UpdateGameResult(emptyGame.Key.Id, GameState.Declined);
                    }

                    EmptyGames.Remove(emptyGame.Key);
                }
            }
        }

        private void AddEmptyGame(Game game)
        {
            TimeSpan currentUtcTimeSpan = DateTime.UtcNow - DateTime.UtcNow.Date;

            if (!EmptyGames.Keys.Any(existingGame => existingGame.Id == game.Id))
            {
                EmptyGames.Add(game, currentUtcTimeSpan);
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Game Tracking Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
