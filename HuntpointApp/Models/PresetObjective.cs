using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Models
{
    public partial class PresetObjective : ObservableObject
    {

        bool _isChecked;

        [JsonIgnore]
        public bool IsChecked { get => _isChecked; set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); } }

        bool _isDraging;

        [JsonIgnore]
        public bool IsDraging { get => _isDraging; set { _isDraging = value; OnPropertyChanged(nameof(IsDraging)); } }


        string _notes;

        [JsonIgnore]
        public string Notes { get => _notes; set { _notes = value; OnPropertyChanged(nameof(Notes)); } }


        string _name;

        [JsonProperty(PropertyName = "name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }

        string _group;

        [JsonProperty(PropertyName = "group")]
        public string Group { get => _group; set { _group = value; OnPropertyChanged(nameof(Group)); } }

        int _score;

        [JsonProperty(PropertyName = "score")]
        public int Score { get => _score; set { _score = value; OnPropertyChanged(nameof(Score)); } }

        ObjectiveType _type = ObjectiveType.Regular;

        [JsonProperty(PropertyName = "type")]
        public ObjectiveType Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }


        ShiftingEarhType _shiftingEarth = ShiftingEarhType.Default;

        [JsonProperty(PropertyName = "shiftingEarth")]
        public ShiftingEarhType ShiftingEath { get => _shiftingEarth; set { _shiftingEarth = value; OnPropertyChanged(nameof(ShiftingEarhType)); } }

    }
    

    public enum ObjectiveType
    {
        Regular= 0,
        Rare = 1,
        Unique = 2,
        Legendary = 3
    }

}
