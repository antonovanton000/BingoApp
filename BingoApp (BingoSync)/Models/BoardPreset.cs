using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace BingoApp.Models
{
    public partial class BoardPreset : ObservableObject
    {
        [ObservableProperty]
        string presetName = default!;

        [ObservableProperty]
        string filePath = default!;


        [ObservableProperty]
        string json = default!;

        [ObservableProperty]
        int squareCount;

        [ObservableProperty]
        ObservableCollection<PresetSquare> squares = new ObservableCollection<PresetSquare>();
                
        [JsonIgnore]
        [ObservableProperty]
        bool isPresetNameError;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonEmpty;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonError;
        
        public void LoadSquares()
        {
            if (!string.IsNullOrEmpty(Json))
            {
                Squares = JsonConvert.DeserializeObject<ObservableCollection<PresetSquare>>(Json);
            }
        }

        public List<PresetSquare> GetSquares()
        {
            var list = new List<PresetSquare>();
            list.AddRange(Squares.Where(i => string.IsNullOrEmpty(i.Group)));
            foreach (var group in Squares.GroupBy(i => i.Group).Where(i => !string.IsNullOrEmpty(i.Key)))
            {
                if (group.Count() >0)
                    list.Add(group.OrderBy(x => Guid.NewGuid()).First());
            }
            return list.OrderBy(x => Guid.NewGuid()).ToList();
        }

        [ObservableProperty]
        bool isCustom;

        [ObservableProperty]
        string game = default!;

        public string PresetAndGame => $"{Game} - {PresetName}"; 
    }
}
