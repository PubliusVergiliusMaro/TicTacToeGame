using TicTacToeGame.Domain.Models;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.WebUI.Services.RoomBackgroundServices
{
    public class RoomBackgroundService : BackgroundService
    {
        private readonly Dictionary<Room, (Timer, Action)> _openedRooms = new();
        public Action? OnRoomDeleted { get; set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Perform background tasks here

                await Task.Delay(5000, stoppingToken); // Example delay, adjust as needed
            }
        }
        public Dictionary<Room, (Timer, Action)> OpenedRooms => _openedRooms;
        public void AddRoom(Room room, Action onRoomDeleted)
        {
            Timer timer = new Timer(10_000); // Example 5 seconds
            timer.Elapsed += (sender, e) => DeleteRoom(room);
            timer.AutoReset = false;
            timer.Start();
            _openedRooms.Add(room, (timer, onRoomDeleted));
        }

        public void DeleteRoom(Room room)
        {
            if (_openedRooms.ContainsKey(room))
            {
                var (timer, onRoomDeleted) = _openedRooms[room];
                timer.Stop();
                timer.Dispose();
                _openedRooms.Remove(room);

                onRoomDeleted?.Invoke();
            }
        }
    }
}

