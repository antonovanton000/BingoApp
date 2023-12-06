﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public class ActiveRoomModel
    {
        public string RoomId { get; set; }

        public string RoomName { get; set; }

        public string FilePath { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
