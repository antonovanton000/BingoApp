namespace SocketServer.Models
{
    public class BingoBoard
    {
        public string RoomId { get; set; }

        public List<BoardSquare> Squares { get; set; } = new();

        public DateTime CreationDate { get; set; }
    }

    public class BoardSquare
    {
        public string Slot { get; set; }

        public string Name { get; set; }
    }
}
