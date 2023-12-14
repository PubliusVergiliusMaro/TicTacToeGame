using System.Security.Claims;
using TicTacToeGame.Domain.Constants;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using TicTacToeGame.Domain.Repositories;
using Timer = System.Timers.Timer;

namespace TicTacToeGame.Services.RoomServices
{
    public class RoomManagerService
    {
        private readonly RoomRepository _roomRepository;
        private readonly GameRepository _gameRepository;

        private readonly Dictionary<Room, (Timer, Action)> _openedRooms = new();
        public Dictionary<Room, (Timer, Action)> OpenedRooms => _openedRooms;

        public Action? OnRoomDeleted { get; set; }

        public RoomManagerService(RoomRepository roomRepository, GameRepository gameRepository)
        {
            _roomRepository = roomRepository;
            _gameRepository = gameRepository;
        }
        public void AddRoom(Room room, Action onRoomDeleted)
        {
            Timer timer = new Timer(HostRoomConstants.WAITING_TIME * 1000); 
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

        public int CreateGame(int joinedRoomId, ClaimsPrincipal user, Player joinedPlayer)
        {
            Room room = OpenedRooms.Keys.First(r => r.ConnectionId == joinedRoomId);
            room.IsOpen = false;
            int roomId = _roomRepository.AddEntity(room);

            Game game = new Game() 
            {
                PlayerHostId = user.Claims.First().Value.ToString(),
                PlayerGuestId = joinedPlayer.Id,
                RoomId = roomId,
                GameResult = GameState.Starting,
                CurrentTurn = PlayerType.Host
            };

            _gameRepository.AddEntity(game);

            return roomId;
        }   
    }
}
