using BingoApp.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp;

public class BingoAppLocalSignalRHub : Hub
{
    public BingoAppLocalSignalRHub()
    {
        App.LocalSignalRHub = this;
        CurrentRoom = App.CurrentRoom;
        if (CurrentRoom != null)
        {
            CurrentRoom.OnMarkSquareRecieved += CurrentRoom_OnMarkSquareRecieved;
            CurrentRoom.OnUnhideGameStep += CurrentRoom_OnUnhideGameStep;
            CurrentRoom.OnChangeGameStep += CurrentRoom_OnChangeGameStep;
            CurrentRoom.OnBoardReveal += CurrentRoom_OnBoardReveal;
            CurrentRoom.OnPlayerConnected += CurrentRoom_OnPlayerConnected;
            CurrentRoom.OnPlayerDisconnected += CurrentRoom_OnPlayerDisconnected;
            CurrentRoom.OnPlayerChangeColor += CurrentRoom_OnPlayerChangeColor;
            CurrentRoom.OnRoomClosed += CurrentRoom_OnRoomClosed;
            CurrentRoom.OnUpdatePlayerGoals += CurrentRoom_OnUpdatePlayerGoals;
            CurrentRoom.OnNewBoardGenerated += CurrentRoom_OnNewBoardGenerated;
            CurrentRoom.OnRefreshRoom += CurrentRoom_OnRefreshRoom;
        }
    }

    private async void CurrentRoom_OnRefreshRoom(object? sender, EventArgs e)
    {
        await SendRefreshPage();
    }

    private async void CurrentRoom_OnNewBoardGenerated(object? sender, EventArgs e)
    {
        await SendRefreshPage();
    }

    private async void CurrentRoom_OnUpdatePlayerGoals(object? sender, PlayerScore[] e)
    {
        await SendPlayersScore(e);
    }

    private async void CurrentRoom_OnRoomClosed(object? sender, Player e)
    {
        await SendRoomClosed();
    }

    private async void CurrentRoom_OnPlayerChangeColor(object? sender, Player e)
    {
        await SendPlayerChangeColor(e.Uuid, e.Color);
    }

    private async void CurrentRoom_OnPlayerDisconnected(object? sender, Player e)
    {
        await SendPlayerDisconnected(e.Uuid);
    }

    private async void CurrentRoom_OnPlayerConnected(object? sender, Player e)
    {
        await SendPlayerConnected(e);
    }

    private async void CurrentRoom_OnBoardReveal(object? sender, EventArgs e)
    {
        await SendRevealBoard();
    }

    private async void CurrentRoom_OnChangeGameStep(object? sender, ChangeGameSquareeEventArgs e)
    {
        await SendChangeSquare(e.SlotId, e.NewSquareName);
    }

    private async void CurrentRoom_OnUnhideGameStep(object? sender, UnhideGameSquareEventArgs e)
    {
        await SendUnhideSquare(e.SlotId);
    }

    private async void CurrentRoom_OnMarkSquareRecieved(object? sender, Square e)
    {
        await SendMarkSquare(e);
    }

    public Room? CurrentRoom { get; private set; }
    
    public event EventHandler<string>? OnMarkSquareRecieved;
    
    public event EventHandler? OnRevealBoardRecieved;

    public async Task InitRoom()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
        }
        else
        {
            var json = JsonConvert.SerializeObject(CurrentRoom, Formatting.Indented);
            await Clients.All.SendAsync("RecieveRoomInfo", json);
        }
    }

    public async Task SendMarkSquare(Square square)
    {
        if (Clients == null) return;

        var json = JsonConvert.SerializeObject(square, Formatting.Indented);
        await Clients.All.SendAsync("MarkSquare", json);
    }
    
    public async Task SendStartTimeStarted(int startTimeSeconds)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        
        await Clients.All.SendAsync("StartTimeStarted", startTimeSeconds);
    }

    public async Task SendAfterRevealTimeStarted(int afterRevealSeconds)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("AfterRevealTimeStarted", afterRevealSeconds);
    }

    public async Task SendRevealBoard()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("RevealBoard", CurrentRoom.RoomId);
    }

    public async Task SendGameStarted()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("StartGame", CurrentRoom.RoomId);
    }

    public async Task SendGameStoped()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("StopGame", CurrentRoom.RoomId);
    }

    public async Task SendTimerStart()
    {
        if (Clients == null) return;
        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }

        await Clients.All.SendAsync("StartTimer", CurrentRoom.RoomId);
    }

    public async Task SendTimerPause()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("PauseTimer", CurrentRoom.RoomId);
    }

    public async Task SendTimerResume(int currentTime)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("ResumeTimer", currentTime);
    }

    public async Task SendTimerReset()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("ResetTimer", CurrentRoom.RoomId);
    }
    
    public void MarkSquare(string slotId)
    {
        OnMarkSquareRecieved?.Invoke(this, slotId); 
    }
    public void RevealBoard()
    {
        OnRevealBoardRecieved?.Invoke(this, EventArgs.Empty);
    }

    public async Task SendNewChatEvent(Event @event)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }

        var json = JsonConvert.SerializeObject(@event);
        await Clients.All.SendAsync("NewEvent", json);
    }


    public async Task SendUnhideSquare(string slotId)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("UnhideSquare", slotId);
    }

    public async Task SendChangeSquare(string slotId, string newSquareName)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("ChangeSquare", slotId, newSquareName);
    }

    public async Task SendPlayerConnected(Player player)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        var json = JsonConvert.SerializeObject(player);
        await Clients.All.SendAsync("PlayerConnected", json);
    }

    public async Task SendPlayerDisconnected(string playerId)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }        
        await Clients.All.SendAsync("PlayerDisconnected", playerId);
    }

    public async Task SendPlayerChangeColor(string playerId, BingoColor color)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }
        await Clients.All.SendAsync("PlayerChangeColor", playerId, (int)color);
    }

    public async Task SendPlayersScore(PlayerScore[] scores)
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }

        var json = JsonConvert.SerializeObject(scores);
        await Clients.All.SendAsync("UpdatePlayerScores", json);
    }

    public async Task SendRefreshPage()
    {
        if (Clients == null) return;

        if (CurrentRoom == null)
        {
            await Clients.All.SendAsync("NoRoom");
            return;
        }        
        await Clients.All.SendAsync("RefreshPage");
    }

    public async Task SendRoomClosed()
    {
        if (Clients == null) return;
        await Clients.All.SendAsync("NoRoom");
    }

    public async Task SendPong()
    {
        if (Clients == null) return;

        await Clients.All.SendAsync("Test", "pong");
    }

    public async Task Test(string ping)
    {
        if (Clients == null) return;

        if (ping == "ping")
            await Clients.All.SendAsync("Test", "pong");
    }


}
