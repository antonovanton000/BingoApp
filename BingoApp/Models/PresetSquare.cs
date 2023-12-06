using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Models
{
    public partial class PresetSquare : ObservableObject
    {
                
        bool _isChecked;
        
        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); } }

        string _name;

        [JsonProperty(PropertyName = "name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }

    }
}
