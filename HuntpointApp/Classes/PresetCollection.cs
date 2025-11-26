using HuntpointApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuntpointApp.Classes;

public class PresetCollection
{
    public static async Task<List<BoardPreset>> GetPresetsAsync()
    {
        var presets = new List<BoardPreset>();
        var folderPath = Path.Combine(App.Location, "Presets");
        if (!Directory.Exists(folderPath))
            return presets;

        var folders = Directory.GetDirectories(folderPath);
        foreach (var folder in folders)
        {
            var gameName = new DirectoryInfo(folder).Name;
            try
            {
                var files = Directory.GetFiles(folder, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var jarr = JArray.Parse(json);
                    var presetFileName = Path.GetFileNameWithoutExtension(file);
                    var preset = new BoardPreset()
                    {
                        PresetName = presetFileName,
                        FilePath = file,
                        Json = json,
                        ObjectivesCount = jarr.Count,
                        Game = gameName,                        
                    };
                    preset.LoadObjectives();
                    presets.Add(preset);
                }
            }
            catch (Exception)
            {
            }
        }

        return presets;
    }

    public static async Task<List<BoardPreset>> GetCustomPresets()
    {
        var presets = new List<BoardPreset>();
        var folderPath = Path.Combine(App.Location, "Presets");
        if (!Directory.Exists(folderPath))
            return presets;

        var folders = Directory.GetDirectories(folderPath);
        foreach (var folder in folders.Where(i => Path.GetDirectoryName(i) == "Custom"))
        {
            try
            {
                var files = Directory.GetFiles(folder, "*.json");

                foreach (var file in files)
                {
                    var json = await System.IO.File.ReadAllTextAsync(file);
                    var jarr = JArray.Parse(json);
                    var presetFileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    var preset = new BoardPreset()
                    {
                        PresetName = presetFileName,
                        FilePath = file,
                        Json = json,
                        ObjectivesCount = jarr.Count,
                        Game = "Custom",                        
                    };
                    preset.LoadObjectives();
                    presets.Add(preset);
                }
            }
            catch (Exception)
            {
            }
        }

        return presets;
    }

    public static async Task<List<Game>> GetGamesAsync()
    {
        var games = new List<Game>();
        var folderPath = System.IO.Path.Combine(App.Location, "Presets");
        if (!Directory.Exists(folderPath))
            return games;

        var folders = System.IO.Directory.GetDirectories(folderPath);
        foreach (var folder in folders.Where(i => Path.GetDirectoryName(i) != "Custom"))
        {
            var game = new Game() { Name = new DirectoryInfo(folder).Name };
            try
            {
                var files = Directory.GetFiles(folder, "*.json");

                foreach (var file in files)
                {
                    var json = await System.IO.File.ReadAllTextAsync(file);
                    var jarr = JArray.Parse(json);
                    var presetFileName = Path.GetFileNameWithoutExtension(file);
                    var preset = new BoardPreset()
                    {
                        PresetName = presetFileName,
                        FilePath = file,
                        Json = json,
                        ObjectivesCount = jarr.Count,
                        Game = game.Name,                        
                    };
                    preset.LoadObjectives();
                    game.Presets.Add(preset);
                }
            }
            catch (Exception)
            {
            }
            games.Add(game);
        }

        return games;
    }

    public static async Task RefreshGamePresets(Game game)
    {
        game.Presets.Clear();
        var presets = new List<BoardPreset>();
        var folderPath = Path.Combine(App.Location, "Presets", game.Name);
        if (!Directory.Exists(folderPath))
            return;
        
        try
        {
            var files = Directory.GetFiles(folderPath, "*.json");

            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var jarr = JArray.Parse(json);
                var presetFileName = Path.GetFileNameWithoutExtension(file);
                var preset = new BoardPreset()
                {
                    PresetName = presetFileName,
                    FilePath = file,
                    Json = json,
                    ObjectivesCount = jarr.Count,
                    Game = game.Name,                    
                };
                preset.LoadObjectives();
                presets.Add(preset);
            }
        }
        catch (Exception)
        {
        }

        foreach (var item in presets)
        {
            game.Presets.Add(item);
        }
    }

    public static async Task<List<PresetObjective>> GetPresetObjectivesByGameAndPresetName(string game, string presetName)
    {
        var folderPath = Path.Combine(App.Location, "Presets", game);        
        var presetFile = Path.Combine(folderPath, presetName + ".json");
        if (!File.Exists(presetFile))
            return [];

        var json = await System.IO.File.ReadAllTextAsync(presetFile);
        var jarr = JArray.Parse(json);
        var presetFileName = System.IO.Path.GetFileNameWithoutExtension(presetFile);
        var preset = new BoardPreset()
        {
            PresetName = presetFileName,
            FilePath = presetFile,
            Json = json,
            ObjectivesCount = jarr.Count,
            Game = "Custom",            
        };
        preset.LoadObjectives();

        return preset.Objectives.ToList();
    }

}
