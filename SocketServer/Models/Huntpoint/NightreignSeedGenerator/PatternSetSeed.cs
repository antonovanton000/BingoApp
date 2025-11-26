using RandomizerCommon;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace NightreignRandomizer.Models;

public class PatternSeedSet
{
    private readonly List<int> setWeights;
    private readonly Dictionary<int, List<uint>> seedCache = new Dictionary<int, List<uint>>();
    private readonly System.Random random = new System.Random();
    private const int CacheSize = 5;
    private const int MaxTries = 5000;
    private static readonly Dictionary<NightreignData.RareMap, int> ConjureOffsets = new Dictionary<NightreignData.RareMap, int>()
    {
        [NightreignData.RareMap.Mountaintop] = 20,
        [NightreignData.RareMap.Crater] = 25,
        [NightreignData.RareMap.Rotted_Woods] = 30,
        [NightreignData.RareMap.Noklateo] = 35
    };

    public PatternSeedSet(IEnumerable<int> setWeights)
    {
        this.setWeights = Enumerable.ToList<int>(setWeights);
    }

    public uint GetRandomSeed() => (uint)this.random.Next();

    public bool TryGetSeed(int pattern, NightreignData.RareMap conjured, out uint seed)
    {
        List<uint> uintList1 = new();
        if (conjured == NightreignData.RareMap.Default && this.seedCache.TryGetValue(pattern, out uintList1) && uintList1.Count > 0)
        {
            seed = uintList1[uintList1.Count - 1];
            uintList1.RemoveAt(uintList1.Count - 1);
            return true;
        }
        for (int index = 0; index < 5000; ++index)
        {
            uint seed1 = (uint)this.random.Next();
            int pattern1 = this.GetPattern(seed1, conjured);
            if (pattern1 == pattern)
            {
                seed = seed1;
                return true;
            }
            if (conjured == NightreignData.RareMap.Default)
            {
                List<uint> uintList2 = new();
                if (this.seedCache.TryGetValue(pattern1, out uintList2))
                {
                    if (uintList2.Count < 5)
                        uintList2.Add(seed1);
                }
                else
                {
                    Dictionary<int, List<uint>> seedCache = this.seedCache;
                    int num = pattern1;
                    List<uint> uintList3 = new List<uint>(5);
                    uintList3.Add(seed1);
                    seedCache[num] = uintList3;
                }
            }
        }
        seed = 0U;
        return false;
    }

    public int GetPattern(uint seed, NightreignData.RareMap conjured)
    {
        int num1 = 0;
        int num2 = 20;
        uint val = new SFMT((int)seed).NextUInt32();
        int num3;
        if (PatternSeedSet.ConjureOffsets.TryGetValue(conjured, out num3))
        {
            num1 = num3;
            num2 = 5;
        }
        else
        {
            SFMT sfmt = new SFMT((int)seed);
            int num4 = Enumerable.Sum((IEnumerable<int>)this.setWeights);
            int num5 = PatternSeedSet.RandInRange(val, 0, num4 - 1);
            int num6 = -1;
            int num7 = 0;
            for (int index = 0; index < this.setWeights.Count; ++index)
            {
                num7 += this.setWeights[index];
                if (num7 > num5)
                {
                    num6 = index;
                    break;
                }
            }
            if (num6 == -1)
                throw new Exception($"Internal error: bad weight {num5}/{num7} for pattern sets [{string.Join(", ", new object[1]
                {
          (object) num4
                })}]");
            if (num6 > 0)
            {
                num2 = 5;
                num1 = 20 + (num6 - 1) * 5;
            }
        }
        return num1 + PatternSeedSet.RandInRange(val, 0, num2 - 1);
    }

    private static int RandInRange(uint val, int start, int end)
    {
        int num = end - start + 1;
        return start + (int)((long)val % (long)num);
    }
}
