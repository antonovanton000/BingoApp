using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class RoomLockoutMode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static GameMode ToGameMode(int id)
        {
            switch (id)
            {
                case 1:
                    return GameMode.Other;
                case 2:
                    return GameMode.Lockout;
                case 3:
                    return GameMode.Blackout;
                case 4:
                    return GameMode.TripleBingo;                
            }

            return GameMode.Other;
        }

        public static ObservableCollection<RoomLockoutMode> All => new ObservableCollection<RoomLockoutMode>()
        {
            //new RoomLockoutMode() { Id = 1, Name = App.Current.FindResource("mes_nonlockout").ToString() },
            new RoomLockoutMode() { Id = 2, Name = App.Current.FindResource("mes_lockout").ToString() },
            new RoomLockoutMode() { Id = 3, Name = App.Current.FindResource("mes_blackout").ToString() },          
            new RoomLockoutMode() { Id = 4, Name = App.Current.FindResource("mes_triplebingo").ToString() }
        };
    }
}
