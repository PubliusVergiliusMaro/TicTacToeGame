using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.Services.RoomServices
{
    public class TemporaryRoomService
    {
        public event Action UpdateComponent;
        public Room Room { get; set; }

        // Timers
        public Timer Stopwatchtimer;
        public int WaitingTime = HostRoomConstants.WAITING_TIME;
        public bool IsTimerElapsed = false;

        public string ConnectionId = HostRoomConstants.LOADING_MESSAGE;

        // Initialize Room
        public void CreateRoom()
        {
            WaitingTime = HostRoomConstants.WAITING_TIME;
            IsTimerElapsed = false;
            Stopwatchtimer = new Timer(1000);
            Stopwatchtimer.AutoReset = true;
            Stopwatchtimer.Elapsed += (sender, e) =>
            {
                WaitingTime--;
                if (WaitingTime == 0)
                {
                    Stopwatchtimer.Stop();
                    Stopwatchtimer.Dispose();
                    IsTimerElapsed = true;
                }
                UpdateComponent?.Invoke();
            };
            Stopwatchtimer.Start();

            Room = new Room()
            {
                ConnectionId = new Random().Next(100_000, 1_000_000),
                IsOpen = true
            };
            ConnectionId = Room.ConnectionId.ToString();
        }

        public bool CheckIfItIsCurrentRoom(int connectionId)
        {
            return Room.ConnectionId == connectionId;
        }
    }
}
