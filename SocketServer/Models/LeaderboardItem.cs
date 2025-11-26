namespace SocketServer.Models
{
    public class LeaderboardItem
    {
        public string PlayerName { get; set; } = default!;

        public int GamesCount { get; set; }

        public int WinsCount { get; set; }

        public string PresetName { get; set; }

        public string GameName { get; set; }
    }
}
