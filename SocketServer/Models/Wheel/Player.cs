using System.Text.RegularExpressions;

namespace SocketServer.Models.Wheel
{
    public class Player
    {

        public Player() { }

        public Player(string id, string nickName)
        {
            Id = id;
            NickName = nickName;                        
        }

        public string Id { get; set; } = default!;
        public string NickName { get; set; } = default!;                
        public int Score { get; set; } = 0;        
        public bool IsPlayerTurn { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public int PlayerIndex { get; set; } = 0;
    }
}
