
namespace SocketServer.Models.Bingo
{
    public class Player
    {

        public Player() { }

        public Player(string id, string nickName)
        {
            Id = id;
            NickName = nickName;
            Color = BingoColor.red;
            IsSpectator = false;            
        }

        public string Id { get; set; } = default!;
        public string NickName { get; set; } = default!;
        public BingoColor Color { get; set; } = BingoColor.red;                
        public bool IsSpectator { get; set; }
        public bool IsBoardRevealed { get; set; } = false;
        public int SquaresCount { get; set; } = 0;
        public int LinesCount { get; set; } = 0;
        public int PotentialBingosCount { get; set; } = 0;

        public string? SelectedBingoLine { get; set; } = null;
    }
}
