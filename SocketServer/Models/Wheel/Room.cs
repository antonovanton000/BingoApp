using Newtonsoft.Json;
using NightreignRandomizer.Models;
using RandomizerCommon;
using SocketServer.Models.Huntpoint.NightreignSeedGenerator;
using System;
using System.Text.RegularExpressions;

namespace SocketServer.Models.Wheel
{
    public class Room
    {
        public Room()
        {
            this.RoomId = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            this.CreaionDate = DateTime.UtcNow;
        }

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

        [JsonIgnore]
        public string DebufsJson { get; set; } = default!;

        public Wheel EarlyWheel { get; set; } = new();
        public Wheel MiddleWheel { get; set; } = new();
        public Wheel EndWheel { get; set; } = new();
        public bool IsGameStarted { get; set; } = false;
        public List<Player> Players { get; set; } = new();        
        public DateTime CreaionDate { get; set; }
        public string CurrentDebuff { get; set; } = string.Empty;

        public void GenerateWheelsFromJson(string json)
        {
            var presetObjectives = JsonConvert.DeserializeObject<List<PresetObjective>>(json);
            if (presetObjectives == null || presetObjectives.Count == 0)
            {
                throw new ArgumentException("Invalid JSON format or empty preset objectives.");
            }

            var ram = new Random();
            // Shuffle the preset objectives
            var earlyObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.EarlyGame).OrderBy(i => ram.NextDouble()).Take(6).ToList();
            var middleObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.MiddleGame).OrderBy(i => ram.NextDouble()).Take(6).ToList();
            var endObjectives = presetObjectives?.Where(i => i.Type == ObjectiveType.EndGame).OrderBy(i => ram.NextDouble()).Take(6).ToList();

            EarlyWheel.Objectives.Clear();
            MiddleWheel.Objectives.Clear();
            EndWheel.Objectives.Clear();

            var modifierSlot = 1;
            var slot = 1;
            if (earlyObjectives != null && earlyObjectives.Count == 6)
            {
                foreach (var item in earlyObjectives)
                {
                    EarlyWheel.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        PlayerIds = new List<string>(),
                        Modifiers = item.Modifiers.Select(m => new ObjectiveModifier
                        {
                            Name = m.Name,
                            Points = m.Points,
                            PlayerIds = new List<string>(),
                            Slot = $"slot{slot}_mod{modifierSlot++}"
                        }).ToList()
                    });
                    slot++;
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough early objectives.");
            }

            modifierSlot = 1;
            if (middleObjectives != null && middleObjectives.Count == 6)
            {
                foreach (var item in middleObjectives)
                {
                    MiddleWheel.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        PlayerIds = new List<string>(),
                        Modifiers = item.Modifiers.Select(m => new ObjectiveModifier
                        {
                            Name = m.Name,
                            Points = m.Points,
                            PlayerIds = new List<string>(),
                            Slot = $"slot{slot}_mod{modifierSlot++}"
                        }).ToList()
                    });
                    slot++;
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough middle objectives.");
            }

            modifierSlot = 1;
            if (endObjectives != null && endObjectives.Count == 6)
            {
                foreach (var item in endObjectives)
                {
                    EndWheel.Objectives.Add(new Objective
                    {
                        Slot = $"slot{slot}",
                        Name = item.Name,
                        Score = item.Score,
                        Type = item.Type,
                        PlayerIds = new List<string>(),
                        Modifiers = item.Modifiers.Select(m => new ObjectiveModifier
                        {
                            Name = m.Name,
                            Points = m.Points,
                            PlayerIds = new List<string>(),
                            Slot = $"slot{slot}_mod{modifierSlot++}"
                        }).ToList()
                    });
                    slot++;
                }
            }
            else
            {
                throw new ArgumentException("Invalid JSON format or not enough end objectives.");
            }
        }

        public void SetRandomDebuff()
        {
            var debuffs = JsonConvert.DeserializeObject<List<string>>(DebufsJson);
            if (debuffs == null || debuffs.Count == 0)
            {
                throw new ArgumentException("Invalid JSON format or empty debuffs.");
            }

            var random = new Random();
            int index = random.Next(debuffs.Count);
            CurrentDebuff = debuffs[index];
        }

        public Objective? FindObjectiveBySlot(string slot)
        {
            var objective = EarlyWheel.Objectives.FirstOrDefault(o => o.Slot == slot) ??
                            MiddleWheel.Objectives.FirstOrDefault(o => o.Slot == slot) ??
                            EndWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
            
            return objective;
        }

        public void RemoveObjectiveBySlot(string slot)
        {
            var obj = EarlyWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
            if (obj != null)
            {
                obj.IsCompleted = true;
                return;
            }
            obj = MiddleWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
            if (obj != null)
            {
                obj.IsCompleted = true;
                return;
            }
            obj = EndWheel.Objectives.FirstOrDefault(o => o.Slot == slot);
            if (obj != null)
            {
                obj.IsCompleted = true;
            }
        }
    }
}
