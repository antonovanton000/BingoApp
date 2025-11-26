using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalWebServerTest
{
    public class BingoAppLocalSignalRHub : Hub
    {
        public async Task InitRoom()
        {
            var json = await System.IO.File.ReadAllTextAsync("testBoard.json");
            await Clients.All.SendAsync("RecieveRoomInfo", json);
        }


    }
}
