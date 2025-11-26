using Newtonsoft.Json;
using NightreignRandomizer.Models;
using RandomizerCommon;
using SocketServer.Models.Huntpoint.NightreignSeedGenerator;
using System;
using System.Text.RegularExpressions;
using static RandomizerCommon.NightreignData;

namespace SocketServer.Models.Huntpoint
{
    public class Room
    {
        public Room()
        {
            this.Board = new Board();
            this.RoomId = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            this.CreaionDate = DateTime.UtcNow;
        }

        [JsonIgnore]
        public string? LegendaryObjective { get; set; }

        [JsonIgnore]
        public string CreatorId { get; set; }

        [JsonIgnore]
        public string CreatorName { get; set; }

        [JsonIgnore]
        public string? RoomPassword { get; set; }

        public RoomSettings RoomSettings { get; set; } = new();
        public string RoomName { get; set; } = default!;

        public string RoomId { get; set; } = default!;

        [JsonIgnore]
        public string PresetJson { get; set; } = default!;

        public string BoardJson { get; set; } = default!;

        public Board Board { get; set; } = new();

        public List<Player> Players { get; set; } = new();

        public DateTime CreaionDate { get; set; }

        public List<Event> ChatMessages { get; set; } = new();
        public uint? NightreignSeed { get; set; } = default!;
        public string NightreignHexSeed => NightreignSeed != null ? $"0x{NightreignSeed:X8}" : "";
        public int? NightreignMapPattern { get; set; } = default!;
        public string PatternDescribe { get; set; } = default!;

        public void GenerateBoardFromJson(string json, string? legendary = null)
        {
            var presetObjectives = JsonConvert.DeserializeObject<List<PresetObjective>>(json);
            if (presetObjectives == null || presetObjectives.Count == 0)
            {
                throw new ArgumentException("Invalid JSON format or empty preset objectives.");
            }
            if (RoomSettings.GameName.Contains("Nightreign"))
            {
                GenerateBoardFromJsonForNightreign(json, legendary);
                return;
            }

            LegendaryObjective = legendary;
            var ram = new Random();
            // Shuffle the preset objectives
            var regularObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Regular).OrderBy(i => ram.NextDouble()).Take(10).ToList();
            var rareObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Rare).OrderBy(i => ram.NextDouble()).Take(6).ToList();
            var uniquObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.Unique).OrderBy(i => ram.NextDouble()).Take(3).ToList();
            PresetObjective? legendaryObjective;
            if (legendary == null)
            {
                legendaryObjective = presetObjectives?.Where(i => i.Type == ObjectiveType.Legendary).OrderBy(i => ram.NextDouble()).FirstOrDefault();
            }
            else
            {
                legendaryObjective = presetObjectives?.FirstOrDefault(i => i.Name.Equals(legendary, StringComparison.OrdinalIgnoreCase) && i.Type == ObjectiveType.Legendary);
                if (legendaryObjective == null)
                {
                    throw new ArgumentException($"Legendary objective '{legendary}' not found in the provided JSON.");
                }
            }

            Board.Objectives.Clear();
            var slot = 1;
            if (regularObjectives != null && regularObjectives.Count == 10)
            {
                foreach (var item in regularObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough regular objectives.");
            }

            if (rareObjectives != null && rareObjectives.Count == 6)
            {
                foreach (var item in rareObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (uniquObjectives != null && uniquObjectives.Count == 3)
            {
                foreach (var item in uniquObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (legendaryObjective != null)
            {
                Board.Objectives.Add(new Objective
                {
                    Slot = $"slot{slot++}",
                    Name = legendaryObjective.Name,
                    Score = legendaryObjective.Score,
                    Type = legendaryObjective.Type,
                    ObjectiveColors = new List<HuntpointColor>()
                });
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough legendary objectives.");
            }

            BoardJson = JsonConvert.SerializeObject(Board.Objectives, Formatting.Indented);
        }

        private void GenerateBoardFromJsonForNightreign(string json, string? legendary = null)
        {
            var presetObjectives = JsonConvert.DeserializeObject<List<PresetObjective>>(json);
            if (presetObjectives == null || presetObjectives.Count == 0)
            {
                throw new ArgumentException("Invalid JSON format or empty preset objectives.");
            }
            LegendaryObjective = legendary;
            var ram = new Random();
            // Shuffle the preset objectives

            var randomShiftingEarth = ram.Next(0, 5);
            var allObjectives = presetObjectives.Where(i => i.ShiftingEath == ShiftingEarhType.Default || i.ShiftingEath == (ShiftingEarhType)randomShiftingEarth).ToList();
            var regularObjectives = allObjectives?.Where(i => i.Type == ObjectiveType.Regular).OrderBy(i => ram.NextDouble()).Take(10).ToList();
            var rareObjectives = allObjectives?.Where(i => i.Type == ObjectiveType.Rare).OrderBy(i => ram.NextDouble()).Take(6).ToList();
            var uniquObjectives = allObjectives?.Where(i => i.Type == ObjectiveType.Unique).OrderBy(i => ram.NextDouble()).Take(3).ToList();
            PresetObjective? legendaryObjective;
            if (legendary == null)
            {
                legendaryObjective = presetObjectives?.Where(i => i.Type == ObjectiveType.Legendary).OrderBy(i => ram.NextDouble()).FirstOrDefault();
            }
            else
            {
                legendaryObjective = presetObjectives?.FirstOrDefault(i => i.Name.Equals(legendary, StringComparison.OrdinalIgnoreCase) && i.Type == ObjectiveType.Legendary);
                if (legendaryObjective == null)
                {
                    throw new ArgumentException($"Legendary objective '{legendary}' not found in the provided JSON.");
                }
            }

            Board.Objectives.Clear();
            var slot = 1;
            if (regularObjectives != null && regularObjectives.Count == 10)
            {
                foreach (var item in regularObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough regular objectives.");
            }

            if (rareObjectives != null && rareObjectives.Count == 6)
            {
                foreach (var item in rareObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (uniquObjectives != null && uniquObjectives.Count == 3)
            {
                foreach (var item in uniquObjectives)
                {
                    Board.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot++}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        ObjectiveColors = new List<HuntpointColor>()
                    });
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough unique objectives.");
            }

            if (legendaryObjective != null)
            {
                Board.Objectives.Add(new Objective
                {
                    Slot = $"slot{slot++}",
                    Name = legendaryObjective.Name,
                    Score = legendaryObjective.Score,
                    Type = legendaryObjective.Type,
                    ObjectiveColors = new List<HuntpointColor>()
                });
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough legendary objectives.");
            }

            if (RoomSettings.IsSameSeedGeneration)
            {
                var set = new PatternSeedSet(new int[5] { 9116, 221, 221, 221, 221 });
                var bossOffset = GetBossOffsetByName(legendaryObjective.Name ?? string.Empty);
                var shiftingEarthOffset = GetShiftingEarthOffset(randomShiftingEarth);
                List<NightreignData.RareMap> rareMapList1 = new List<NightreignData.RareMap>();
                rareMapList1.Add(NightreignData.RareMap.Default);
                rareMapList1.Add(NightreignData.RareMap.Mountaintop);
                rareMapList1.Add(NightreignData.RareMap.Crater);
                rareMapList1.Add(NightreignData.RareMap.Rotted_Woods);
                rareMapList1.Add(NightreignData.RareMap.Noklateo);
                var seed = set.GetRandomSeed();
                var patternMaxOffset = ram.Next(0, GetMaxPatternOffset(randomShiftingEarth));
                var pattern = bossOffset + shiftingEarthOffset + patternMaxOffset;
                int offset = pattern % 40;

                if (set.TryGetSeed(offset, rareMapList1[0], out seed))
                {
                    NightreignSeed = seed;
                    NightreignMapPattern = pattern;
                    PatternDescribe = DescribePattern(pattern);
                }
                else
                {
                    throw new ArgumentException("Failed to generate a valid Nightreign seed.");
                }
            }

            BoardJson = JsonConvert.SerializeObject(Board.Objectives, Formatting.Indented);
        }

        private string DescribePattern(int patternId)
        {
            List<NightreignData.RareMap> MapGroups;
            if (patternId < 0 || patternId >= 320)
                return "pattern";
            
            NightreignData.TargetBoss boss = TargetBosses.From(patternId / 40);
            int num = patternId % 40;
            var name = GetShiftingEarthName(num);
            return $"pattern #{patternId} ({name} for {boss.GetName()})";
        }

        private int GetMaxPatternOffset(int shiftingEarth)
        {
            return shiftingEarth switch
            {
                0 => 20,
                1 => 5,
                2 => 5,
                3 => 5,
                4 => 5,
                _ => 0
            };
        }

        private int GetShiftingEarthOffset(int shiftingEarth)
        {
            return shiftingEarth switch
            {
                0 => 0,
                1 => 20,
                2 => 25,
                3 => 30,
                4 => 35,
                _ => 0
            };
        }

        private string GetShiftingEarthName(int shiftingEarth)
        {
            if (shiftingEarth >= 0 && shiftingEarth <= 19)
                return "Default";
            else if (shiftingEarth >= 20 && shiftingEarth <= 24)
                return "Mountaintop";
            else if (shiftingEarth >= 25 && shiftingEarth <= 29)
                return "Crater";
            else if (shiftingEarth >= 30 && shiftingEarth <= 34)
                return "Rotted Woods";
            else if (shiftingEarth >= 35 && shiftingEarth <= 39)
                return "Noklateo";
            else
                return "";
        }


        private int GetBossOffsetByName(string bossName)
        {
            if (bossName.Contains("трицефал", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }
            else if (bossName.Contains("зияющая пасть", StringComparison.OrdinalIgnoreCase))
            {
                return 40;
            }
            else if (bossName.Contains("разумный мор", StringComparison.OrdinalIgnoreCase))
            {
                return 80;
            }
            else if (bossName.Contains("авгур", StringComparison.OrdinalIgnoreCase))
            {
                return 120;
            }
            else if (bossName.Contains("равновесный зверь", StringComparison.OrdinalIgnoreCase))
            {
                return 160;
            }
            else if (bossName.Contains("рыцарь темного течения", StringComparison.OrdinalIgnoreCase))
            {
                return 200;
            }
            else if (bossName.Contains("излом тумана", StringComparison.OrdinalIgnoreCase))
            {
                return 240;
            }
            else if (bossName.Contains("аспект ночи", StringComparison.OrdinalIgnoreCase))
            {
                return 280;
            }
            else
                return 0;
        }

    }
}
