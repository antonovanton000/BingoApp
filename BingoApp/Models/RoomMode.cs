using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class RoomMode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public GameMode ToGameMode()
        {
            return ToGameMode(this.Id);
        }

        public static GameMode ToGameMode(int id)
        {
            switch (id)
            {
                case 0:
                    return GameMode.Lockout;
                case 1:
                    return GameMode.Blackout;
                case 2:
                    return GameMode.Triple;                
                case 3:
                    return GameMode.Other;
            }

            return GameMode.Other;
        }

        public static ObservableCollection<RoomMode> All => new ObservableCollection<RoomMode>()
        {
            new RoomMode() { Id = 0, Name = App.Current.FindResource("mes_lockout").ToString() },
            new RoomMode() { Id = 1, Name = App.Current.FindResource("mes_blackout").ToString() },          
            new RoomMode() { Id = 2, Name = App.Current.FindResource("mes_triplebingo").ToString() },
            new RoomMode() { Id = 3, Name = App.Current.FindResource("mes_other").ToString() }
        };
    }
}
