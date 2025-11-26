using System.Text.RegularExpressions;

namespace SocketServer.Models.Huntpoint
{
    public class Player
    {

        public Player() { }

        public Player(string id, string nickName)
        {
            Id = id;
            NickName = nickName;
            Color = HuntpointColor.red;
            IsSpectator = false;
        }

        public string Id { get; set; } = default!;
        public string NickName { get; set; } = default!;
        public HuntpointColor Color { get; set; } = HuntpointColor.red;
        public bool IsFinished { get; set; } = false;
        public int Score { get; set; } = 0;
        public bool IsSpectator { get; set; }
    }
}
