using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

namespace SocketServer.Models.Bingo;

public class Room
{
    public Room()
    {
        this.Board = new Board();
        this.RoomId = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
        this.CreaionDate = DateTime.UtcNow;
    }
    
    [JsonIgnore]
    public string CreatorId { get; set; }

    [JsonIgnore]
    public string CreatorName { get; set; }

    [JsonIgnore]
    public string? RoomPassword { get; set; }

    public RoomSettings RoomSettings { get; set; } = new();
    public string RoomName { get; set; } = default!;

    public string RoomId { get; set; } = default!;
    
    [JsonIgnore]
    public string PresetJson { get; set; } = default!;
   
    public Board Board { get; set; } = new();

    public List<Player> Players { get; set; } = new();

    public DateTime CreaionDate { get; set; }

    public List<Event> ChatMessages { get; set; } = new();
    
    public bool IsCreatorMode { get; set; }
    public TimeSpan TimerCounter { get; set; }    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsGameStarted { get; set; }
    public bool IsGameEnded { get; set; }   
    public bool IsHiddenGameInited { get; set; }    
    public int LastChangeMinute { get; set; }
    public bool IsTimerStarted { get; set; }
    public bool IsTimerOnPause { get; set; }
    public bool IsAutoRevealBoardVisible { get; set; }
    public bool IsRevealed { get; set; }
    public int CurrentHiddenStep { get; set; } = 1;

    public void GenerateBoardFromJson()
    {
        var squares = JsonConvert.DeserializeObject<List<PresetSquare>>(PresetJson);
        if (squares == null || squares.Count < 25)
        {
            throw new ArgumentException("Invalid JSON format or empty preset squares.");
        }

        var ram = new Random();
        // Shuffle the preset squares
        var rndSquares = squares.OrderBy(i => ram.NextDouble()).Take(25).ToList();        
        var slot = 1;
        var row = 0;
        var col = 0;

        Board.Squares.Clear();
        foreach (var item in rndSquares)
        {
            Board.Squares.Add(new Square
            {
                Name = item.Name,
                Slot = $"slot{slot++}",
                Row = row,
                Column = col++,
                SquareColors = new List<BingoColor>()
            });
            if (col >= 5)
            {
                col = 0;
                row++;
            }
        }

        if (RoomSettings.ExtraGameMode == ExtraGameMode.Hidden)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    if (y % 2 == 0)
                    {
                        Board.Squares[y * 5 + x].IsHidden = x % 2 == 0;
                    }
                    else
                    {
                        Board.Squares[y * 5 + x].IsHidden = x % 2 != 0;
                    }
                }
            }            

            IsHiddenGameInited = true;
            CurrentHiddenStep = 1;
        }        
    }
}
