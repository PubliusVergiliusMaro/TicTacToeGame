using System.Timers;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Services.RoomServices
{
    public class RoomService : IDisposable
    {
        public Dictionary<Room, (System.Timers.Timer, Action)> OpenedRooms { get; set; } = new();
        public Action? OnRoomDeleted { get; set; }

        public void AddRoom(Room room, Action onRoomDeleted)
        {
            System.Timers.Timer timer = new System.Timers.Timer(5_000);// 30 sec
            timer.Elapsed += (sender, e) => DeleteRoom(room);
            timer.AutoReset = false;
            timer.Start();
            OpenedRooms.Add(room, (timer, onRoomDeleted));
        }
        public void DeleteRoom(Room room)
        {
            if (OpenedRooms.ContainsKey(room))
            {
                var (timer, onRoomDeleted) = OpenedRooms[room];
                timer.Stop();
                timer.Dispose();
                OpenedRooms.Remove(room);

                onRoomDeleted();
            }
        }
        public void Dispose()
        {
            foreach (var (timer, _) in OpenedRooms.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            OpenedRooms.Clear();
        }
    }
}
