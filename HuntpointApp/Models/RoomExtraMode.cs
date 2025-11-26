using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public class RoomExtraMode
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public static ExtraGameMode ToExtraGameMode(int id)
        {
            switch (id)
            {
                case 1:
                    return ExtraGameMode.None;
                case 2:
                    return ExtraGameMode.Hidden;
                case 3:
                    return ExtraGameMode.Changing;                
            }

            return ExtraGameMode.None;
        }

        public static ObservableCollection<RoomExtraMode> All =>
        [
            new RoomExtraMode() { Id = 1, Name = App.Current.FindResource("mes_none").ToString() },
            new RoomExtraMode() { Id = 2, Name = App.Current.FindResource("mes_hidden").ToString() },          
            new RoomExtraMode() { Id = 3, Name = App.Current.FindResource("mes_changing").ToString() }
        ];
    }

    public enum ExtraGameMode
    {
        None,
        Hidden,
        Changing
    }
}
