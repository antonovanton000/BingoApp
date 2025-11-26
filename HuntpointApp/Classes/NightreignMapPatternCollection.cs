using HuntpointApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HuntpointApp.Classes
{
    public class NightreignMapPatternCollection
    {
        public static NightreignMapPatternCollection Instance { get; private set; }

        public List<NightreignMapPattern> MapPatterns { get; private set; } = [];

        public NightreignMapPatternCollection()
        {
            Instance = this;
            Mapping.Init();
        }

        public void LoadPatterns()
        {
            Mapping.Init();
            MapPatterns.Clear();
            var patternsPath = Path.Combine(App.Location, "MapPatterns");
            var bossesFolders = Directory.GetDirectories(patternsPath,"");
            foreach (var bossFolder in bossesFolders.Where(i => Path.GetFileName(i) != "_mapping"))
            {
                var shiftingearthFolders = Directory.GetDirectories(bossFolder);
                foreach (var shiftingearthFolder in shiftingearthFolders)
                {
                    var spawnpointFolders = Directory.GetDirectories(shiftingearthFolder);
                    foreach (var spawnpointFolder in spawnpointFolders)
                    {
                        var files = Directory.GetFiles(spawnpointFolder, "_*.jpg");
                        foreach (var item in files)
                        {
                            var pattern = new NightreignMapPattern()
                            {
                                PatternName = Path.GetFileNameWithoutExtension(item).Replace("_", ""),
                                BossName = Path.GetFileName(bossFolder),
                                ShiftingEarh = Path.GetFileName(shiftingearthFolder),
                                SpawnPoint = Path.GetFileName(spawnpointFolder),
                                ThumbnailPath = item,
                                ImagePath = item.Replace("\\_", "\\"),
                            };
                            MapPatterns.Add(pattern);
                        }
                    }
                }
            }
        }
        public class Mapping
        {
            private static JObject jobj = new JObject();

            public static void Init()
            {
                jobj = JObject.Parse(File.ReadAllText(Path.Combine(App.Location, "MapPatterns", "_mapping", "mapping.json")));
            }

            public static string GetBossName(string name)
            {
                if (jobj == null || !jobj.HasValues)
                    return name;

                var boss = jobj["BossNames"][name];
                if (boss != null && boss.Type == JTokenType.String)
                {
                    return boss.ToString();
                }
                return name;
            }

            public static string GetSpawnPointName(string name)
            {
                if (jobj == null || !jobj.HasValues)
                    return name;

                var boss = jobj["SpawnPoints"][name];
                if (boss != null && boss.Type == JTokenType.String)
                {
                    return boss.ToString();
                }
                return name;
            }

            public static string GetShiftingEarthName(string name)
            {
                if (jobj == null || !jobj.HasValues)
                    return name;

                var boss = jobj["ShiftingEarth"][name];
                if (boss != null && boss.Type == JTokenType.String)
                {
                    return boss.ToString();
                }
                return name;
            }


            public static KeyValuePair<string,string>[] GetAllBosses()
            {
                if (jobj == null || !jobj.HasValues)
                    return [];

                return jobj["BossNames"]?.Select(x => new KeyValuePair<string,string>((x as JProperty).Name, (x as JProperty).Value.ToString())).ToArray() ?? [];
            }

            public static KeyValuePair<string, string>[] GetAllShiftingEarthes()
            {
                if (jobj == null || !jobj.HasValues)
                    return [];

                return jobj["ShiftingEarth"]?.Select(x => new KeyValuePair<string, string>((x as JProperty).Name, (x as JProperty).Value.ToString())).ToArray() ?? [];
            }

            public static KeyValuePair<string, string>[] GetAllSpawnPoints()
            {
                if (jobj == null || !jobj.HasValues)
                    return [];

                return jobj["SpawnPoints"]?.Select(x => new KeyValuePair<string, string>((x as JProperty).Name, (x as JProperty).Value.ToString())).ToArray() ?? [];
            }

            public static Thickness GetSpawnPointMargins(string spawnPoint)
            {
                if (jobj == null || !jobj.HasValues)
                    return new Thickness(0);
             
                var margins = jobj["SpawnPointMargins"]?[spawnPoint];
                if (margins != null && margins.Type == JTokenType.String)
                {
                    var marginArray = margins.ToString().Split(',');
                    return new Thickness(double.Parse(marginArray[0]), double.Parse(marginArray[1]), double.Parse(marginArray[2]), double.Parse(marginArray[3]));
                }
                return new Thickness(0);
            }

        }
    }

}
