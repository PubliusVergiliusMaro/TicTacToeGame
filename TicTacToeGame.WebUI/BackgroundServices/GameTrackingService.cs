using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Repositories;
using TicTacToeGame.Services.GameProcessService;
using Game = TicTacToeGame.Domain.Models.Game;

namespace TicTacToeGame.WebUI.BackgroundServices
{
    public class GameTrackingService : IHostedService, IDisposable
    {
        private int executionCount = 0;

        private readonly ILogger<GameTrackingService> _logger;

        private Timer? _timer = null;

        private readonly GameRepository _gameRepository;

        private readonly Dictionary<Guid, Game> EmptyGames = new();

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

            AddEmptyGame();

            // Remove
            _logger.LogError("Game Tracking Service is working. Iteration: {Count}. Number of games with no players {EmptyGames}", count, EmptyGames.Count);
            //

            _logger.LogInformation("Game Tracking Service is working. Iteration: {Count}. Number of games with no players {EmptyGames}", count, EmptyGames.Count);
        }

        private void ClearEmptyGames()
        {
            foreach (var emptyGame in EmptyGames)
            {
                Domain.Models.Game currentEmptyGame = _gameRepository.GetById(emptyGame.Value.Id);

                if (currentEmptyGame != null)
                {
                    if (currentEmptyGame.GameResult == GameState.Starting)
                    {
                        _gameRepository.UpdateGameResult(emptyGame.Value.Id, GameState.Declined);
                    }

                    EmptyGames.Remove(emptyGame.Key);
                }
            }
        }

        // 
        //private void ClearEmptyGames()
        //{
        //    List<Game> emptyGames = _gameRepository.GetEmptyGames();

        //    foreach (var emptyGame in emptyGames)
        //    {
        //        if (EmptyGames.ContainsKey(emptyGame.UniqueId))
        //        {
        //            _gameRepository.UpdateGameResult(emptyGame.Id, GameState.Declined);
        //            EmptyGames.Remove(emptyGame.UniqueId);
        //        }
        //        else
        //        {
        //            EmptyGames.Add(emptyGame.UniqueId, emptyGame);
        //        }
        //    }
        //}

        private void AddEmptyGame()
        {
            List<Game> emptyGames = _gameRepository.GetEmptyGames();

            foreach (var emptyGame in emptyGames)
            {
                if (!EmptyGames.ContainsKey(emptyGame.UniqueId))
                {
                    EmptyGames.Add(emptyGame.UniqueId, emptyGame);
                }
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
