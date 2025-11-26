using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace HuntpointApp.Models
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
        ObservableCollection<PresetObjective> objectives = [];

        [ObservableProperty]
        [JsonIgnore]
        int objectivesCount;

        [JsonIgnore]
        [ObservableProperty]
        bool isPresetNameError;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonEmpty;

        [JsonIgnore]
        [ObservableProperty]
        bool isJsonError;
        
        public void LoadObjectives()
        {
            if (!string.IsNullOrEmpty(Json))
            {
                Objectives = JsonConvert.DeserializeObject<ObservableCollection<PresetObjective>>(Json);
            }
        }

        public List<PresetObjective> GetObjectives()
        {
            var list = new List<PresetObjective>();
            list.AddRange(Objectives.Where(i => string.IsNullOrEmpty(i.Group)));
            foreach (var group in Objectives.GroupBy(i => i.Group).Where(i => !string.IsNullOrEmpty(i.Key)))
            {
                if (group.Count() >0)
                    list.Add(group.OrderBy(x => Guid.NewGuid()).First());
            }
            return list.ToList();
        }
        
        [ObservableProperty]
        string game = default!;

        public string PresetAndGame => $"{Game} - {PresetName}"; 
    }
}
